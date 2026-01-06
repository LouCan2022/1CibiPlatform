using AIAgent.Features.AskAI;

namespace AIAgent.Services;

public class AIAgentService : IAIAgentService
{
	private readonly Kernel _kernel;
	private readonly ILogger<AIAgentService> _logger;
	private readonly IHubContext<AIAgentHub, IAIClient> _hubContext;
	private readonly SkillRegistry _skillRegistry;
	private readonly ConcurrentDictionary<string, List<(string Role, string Content)>> _conversations = new();
	private readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();

	// Maximum messages to keep in memory per user (messages, not exchanges)
	private const int MaxHistoryMessages = 20;

	public AIAgentService(
		Kernel kernel,
		ILogger<AIAgentService> logger,
		IHubContext<AIAgentHub, IAIClient> hubContext,
		SkillRegistry skillRegistry
		)
	{
		this._kernel = kernel;
		this._logger = logger;
		this._hubContext = hubContext;
		this._skillRegistry = skillRegistry;
	}

	public async Task<AIAnswerDTO> GetAIAnswerAsync(
		string userId,
		string question,
		UploadedFileDto? uploadedFile,
		CancellationToken cancellationToken)
	{
		ProcessExcelInput? processInput = null;
		string? uploadedBase64 = null;
		if (uploadedFile is not null)
		{
			// Convert bytes to base64 (do not validate here)
			uploadedBase64 = Convert.ToBase64String(uploadedFile.Content);
			processInput = new ProcessExcelInput
			{
				FileName = uploadedFile.FileName,
				Base64File = uploadedBase64,
				HeaderRow = 1
			};
		}

		var sem = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

		await sem.WaitAsync(cancellationToken);

		try
		{
			// Retrieve or initialize conversation history
			var history = _conversations.GetOrAdd(userId, _ => new List<(string, string)>());

			// Build conversation context
			string historyText = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));

			// If there's an uploaded file and question suggests processing, use LLM to choose a skill
			if (processInput is not null && !string.IsNullOrWhiteSpace(question) &&
				(question.IndexOf("read", StringComparison.OrdinalIgnoreCase) >= 0 ||
				question.IndexOf("open", StringComparison.OrdinalIgnoreCase) >= 0 ||
				question.IndexOf("parse", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				// Build skills summary from registry
				var skills = _skillRegistry.GetAll().Select(d => $"{d.Name}: {d.ManifestPath ?? "(no manifest)"}");
				var skillsText = string.Join("\n", skills);

				// Decision prompt: ask LLM to pick a skill and return JSON {"skill":"Name","input":{...}}. Use placeholder for uploaded file
				var decisionTemplate = """
                                      You are given a user question and a list of available skills (name: manifest path).
                                      Decide which skill should be called to help the user. Return ONLY valid JSON with two fields: "skill" (the exact skill name) and "input" (an object with the parameters to pass). 
                                      We have two fields inside input, first Base64File set the parameter value to the string "__UploadFile__" for the file content parameter but in result do not include double qoute just __UploadFile__ File.
                                      Second is Filename parameter set the exact file name set the parameter value to the string "__Filename__" for the file content parameter but in result do not include double qoute just __Filename__ File.
                                      DO NOT embed the file here.
                                      Available skills:
                                      {{$skillsText}}
                                      User question:
                                      {{$input}}
                                      Return JSON only.
                                      """;

				var decisionArgs = new KernelArguments { ["input"] = question };
				decisionArgs["skillsText"] = skillsText;

				// Let exceptions propagate to the global handler
				var decisionRes = await _kernel.InvokePromptAsync(decisionTemplate, decisionArgs);
				var decisionRaw = decisionRes?.ToString() ?? string.Empty;

				// Try parse JSON (exceptions will bubble)
				if (!string.IsNullOrWhiteSpace(decisionRaw))
				{
					// extract first JSON object from the response
					var jsonStart = decisionRaw.IndexOf('{');
					var jsonEnd = decisionRaw.LastIndexOf('}');
					if (jsonStart >= 0 && jsonEnd > jsonStart)
					{
						var json = decisionRaw.Substring(jsonStart, jsonEnd - jsonStart + 1);

						// Inject uploaded file base64 if placeholder present
						if (!string.IsNullOrEmpty(uploadedBase64) && json.Contains("__UploadFile__"))
						{
							json = json.Replace("__UploadFile__", JsonSerializer.Serialize(uploadedBase64));
						}

						if (!string.IsNullOrEmpty(processInput.FileName) && json.Contains("__Filename__"))
						{
							json = json.Replace("__Filename__", JsonSerializer.Serialize(processInput.FileName));
						}

						using var doc = JsonDocument.Parse(json);
						var root = doc.RootElement;
						if (root.TryGetProperty("skill", out var skillProp) &&
							skillProp.ValueKind == JsonValueKind.String)
						{
							var skillName = skillProp.GetString()!;
							var inputElem = root.TryGetProperty("input", out var ie) ? ie : default;

							if (!string.IsNullOrWhiteSpace(skillName) &&
								_skillRegistry.TryGet(skillName, out var sd))
							{
								// invoke selected skill with provided input (or empty object)
								JsonElement payload = inputElem.ValueKind == JsonValueKind.Undefined ? JsonSerializer.SerializeToElement(new { }) : inputElem;
								var skillResult = await _skillRegistry.InvokeAsync(skillName, payload, cancellationToken);
								var skillText = skillResult is string ss ? ss : JsonSerializer.Serialize(skillResult);

								// Inject skill result into prompt and ask kernel for final answer
								var finalTemplate = """
                                                       You are a helpful assistant. Use the conversation history and the result of a called skill to answer the user.
                                                       Conversation history:
                                                       {{$historyText}}
                                                       Skill result:
                                                       {{$skillResult}}
                                                       User: {{$input}}
                                                       Assistant:
                                                       """;
								var finalArgs = new KernelArguments { ["input"] = question };
								finalArgs["historyText"] = historyText;
								finalArgs["skillResult"] = skillText;

								var finalRaw = await _kernel.InvokePromptAsync(finalTemplate, finalArgs);
								var finalAnswer = finalRaw?.ToString()?.Trim() ?? string.Empty;

								await _hubContext.Clients.Group(userId).ReceiveAiResponse(finalAnswer);
								history.Add(("User", question));
								history.Add(("Assistant", finalAnswer));
								return new AIAnswerDTO(new List<string> { finalAnswer }, null);
							}
						}
					}
				}
			}

			// No skill chosen or processed; continue with prompt-based completion
			var template = """
                              You are a helpful, honest, and concise assistant.
                              Use the conversation history below to provide context-aware answers.
                              Add some format with your answers to make it more readable.
                              If you don't know, say "I don't know."
                              Conversation history:
                              {{$historyText}}
                              User: {{$input}}
                              Assistant:
                            """;

			var args = new KernelArguments { ["input"] = question };
			args["historyText"] = historyText;

			_logger.LogDebug("Invoking kernel for user {UserId} with history length {HistoryCount}", userId, history.Count);

			// Notify clients that the assistant is "typing"
			_ = _hubContext.Clients.Group(userId).ReceiveTyping(true);

			var raw = await _kernel.InvokePromptAsync(template, args);
			var answer = raw?.ToString()?.Trim() ?? string.Empty;

			// Send full answer to connected clients
			await _hubContext.Clients.Group(userId).ReceiveAiResponse(answer);


			_ = _hubContext.Clients.Group(userId).ReceiveTyping(false);

			// Append new exchange to history 
			history.Add(("User", question));
			history.Add(("Assistant", answer));

			// Trim history if it exceeds maximum allowed messages
			if (history.Count > MaxHistoryMessages)
			{
				while (history.Count > MaxHistoryMessages)
				{
					history.RemoveAt(0);
				}
			}

			return new AIAnswerDTO(new List<string> { answer }, null);
		}
		finally
		{
			sem.Release();
		}
	}

	// Call this on user logout
	public void ClearConversation(string userId)
	{
		_conversations.TryRemove(userId, out _);
		if (_userLocks.TryRemove(userId, out var sem))
		{
			sem.Dispose();
		}

		// Notify connected clients the session was cleared
		_ = _hubContext.Clients.Group(userId).SessionCleared();
	}
}
