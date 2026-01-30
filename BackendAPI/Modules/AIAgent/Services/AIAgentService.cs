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
		string? explicitSkillName,
		CancellationToken cancellationToken)
	{
		var sem = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));
		await sem.WaitAsync(cancellationToken);

		try
		{
			// Notify UI that processing has started
			await _hubContext.Clients.Group(userId).ReceiveTyping(true);

			var history = _conversations.GetOrAdd(userId, _ => new List<(string, string)>());
			string historyText = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));

			// ⭐ Explicit Skill Selection (Deterministic - User/UI controls)
			if (!string.IsNullOrWhiteSpace(explicitSkillName))
			{
				return await InvokeExplicitSkill(
					userId, question, uploadedFile, explicitSkillName,
					history, historyText, cancellationToken);
			}

			// ⭐ No skill selected - Return generic response
			return await HandleGenericQuery(userId, question, history, historyText, cancellationToken);
		}
		finally
		{
			// Notify UI that processing has completed
			await _hubContext.Clients.Group(userId).ReceiveTyping(false);
			sem.Release();
		}
	}

	private async Task<AIAnswerDTO> InvokeExplicitSkill(
		string userId,
		string question,
		UploadedFileDto? uploadedFile,
		string explicitSkillName,
		List<(string Role, string Content)> history,
		string historyText,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("User {UserId} explicitly requested skill: {SkillName}", userId, explicitSkillName);


		//Validate uploadfile if null
		if (uploadedFile == null)
		{
			_logger.LogWarning("No file uploaded for skill {SkillName} by user {UserId}", explicitSkillName, userId);
			throw new InvalidOperationException("A file must be uploaded to use this skill.");
		}


		if (!_skillRegistry.TryGet(explicitSkillName, out var skillDef) || skillDef?.ImplementationType == null)
		{
			var errorMsg = $"Skill '{explicitSkillName}' not found or not implemented.";
			_logger.LogWarning("Skill not found: {SkillName} for user {UserId}", explicitSkillName, userId);

			await _hubContext.Clients.Group(userId).ReceiveAiResponse(errorMsg);

			history.Add(("User", question));
			history.Add(("Assistant", errorMsg));
			TrimHistory(history);

			return new AIAnswerDTO(new List<string> { errorMsg }, null);
		}

		_logger.LogDebug("Creating skill instance for {SkillName} with type {ImplementationType}",
			explicitSkillName, skillDef.ImplementationType.Name);


		//  Create a scope to resolve scoped services like DbContext
		using var scope = _kernel.Services.CreateScope();
		var skillInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, skillDef.ImplementationType);

		if (skillInstance is not ISkill skill)
		{
			throw new InvalidOperationException($"Skill '{explicitSkillName}' does not implement ISkill.");
		}

		// Prepare skill input payload
		var payload = PrepareSkillPayload(
			uploadedFile,
			question,
			historyText,
			skillInstance);


		var analyzeQuestion = await AnalyzetheQuestionIfRelatedToSkillSet(payload);

		if (analyzeQuestion == "Not related.")
		{
			return new AIAnswerDTO(new List<string> { "Your request is not related on the skillset you picked" }, "");
		}


		_logger.LogInformation("Executing skill {SkillName} for user {UserId}", explicitSkillName, userId);

		var skillResult = await skill.RunAsync(JsonSerializer.SerializeToElement(payload), cancellationToken);

		_logger.LogInformation("Skill {SkillName} completed successfully for user {UserId}", explicitSkillName, userId);

		// Format result
		var resultText = FormatSkillResult(skillResult);
		await _hubContext.Clients.Group(userId).ReceiveAiResponse(resultText);

		history.Add(("User", question));
		history.Add(("Assistant", resultText));
		TrimHistory(history);

		return new AIAnswerDTO(new List<string> { resultText }, "");
	}

	private object PrepareSkillPayload(
		UploadedFileDto?
		uploadedFile,
		string question,
		string historyText,
		object skills)
	{
		if (uploadedFile != null)
		{
			return new
			{
				FileName = uploadedFile.FileName,
				Base64File = uploadedFile.Content,
				HeaderRow = 1,
				UserQuestion = question,
				HistoryText = historyText,
				Skills = skills
			};
		}

		return new { UserQuestion = question };
	}


	private async Task<string> AnalyzetheQuestionIfRelatedToSkillSet(object payload)
	{
		var question = payload.GetType().GetProperty("UserQuestion")!.GetValue(payload);
		var historyText = payload.GetType().GetProperty("HistoryText")!.GetValue(payload);
		var skills = payload.GetType().GetProperty("Skills")!.GetValue(payload);


		var template = """
						You should validate if the skills selected is kinda match on users question just dont invoke skill
						i just need you to assess if they are related or not
						
						if the question is not related to skills Name context just answer with "Not related."
							
						Conversation history:
						{{$historyText}}
						
						User: {{$input}}

						Selected Skills:
						{{$skillSelected}}


						Assistant:
						""";


		var args = new KernelArguments
		{
			["input"] = question,
			["historyText"] = historyText,
			["skillSelected"] = skills
		};

		var result = await _kernel.InvokePromptAsync(template, args);

		if (string.IsNullOrEmpty(result?.ToString().Trim()))
		{
			return "";
		}

		return result.ToString().Trim();
	}

	private string FormatSkillResult(object? result)
	{
		if (result == null)
			return "Skill executed successfully with no output.";

		// Check if it's a ProcessExcelResult or similar structure
		var resultType = result.GetType();
		var successProp = resultType.GetProperty("Success");
		var messageProp = resultType.GetProperty("Message");
		var rowsProp = resultType.GetProperty("Rows");

		if (successProp != null && messageProp != null)
		{
			var success = successProp.GetValue(result) as bool? ?? false;
			var message = messageProp.GetValue(result) as string ?? string.Empty;

			if (!success)
				return $"❌ {message}";

			var output = new StringBuilder();
			output.AppendLine($"✅ {message}");

			if (rowsProp != null)
			{
				var rows = rowsProp.GetValue(result) as IEnumerable<object>;
				if (rows != null && rows.Any())
				{
					var rowCount = rows.Count();
					output.AppendLine($"\n📊 Processed {rowCount} row(s)");
				}
			}

			return output.ToString();
		}

		return result.ToString() ?? "Skill executed successfully.";
	}

	private void TrimHistory(List<(string Role, string Content)> history)
	{
		while (history.Count > MaxHistoryMessages)
		{
			history.RemoveAt(0);
		}
	}

	private async Task<AIAnswerDTO> HandleGenericQuery(
		string userId,
		string question,
		List<(string Role, string Content)> history,
		string historyText,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling generic query for user {UserId}", userId);

		// ⭐ LIMITED RESPONSE - Only answer about available skills
		var availableSkills = _skillRegistry.GetAll()
			.Select(s => $"- {s.Name}: {s.MethodName ?? "No description"}")
			.ToList();

		var template = """
						You are an AI assistant with LIMITED capabilities.
						You can ONLY help with these specific tasks:
						{{$availableSkills}}
						
						If the user asks for something outside these capabilities, politely inform them and provide all the skills available:
						"I can only help with the listed tasks. Please select a skill to proceed."
						
						Conversation history:
						{{$historyText}}
						
						User: {{$input}}
						Assistant:
						""";

		var args = new KernelArguments
		{
			["input"] = question,
			["historyText"] = historyText,
			["availableSkills"] = string.Join("\n", availableSkills)
		};

		var result = await _kernel.InvokePromptAsync(template, args);
		var answer = result?.ToString()?.Trim() ??
					 "I can only help with specific data processing tasks. Please select a skill or upload a file.";

		await _hubContext.Clients.Group(userId).ReceiveAiResponse(answer);

		history.Add(("User", question));
		history.Add(("Assistant", answer));
		TrimHistory(history);

		return new AIAnswerDTO(new List<string> { answer }, null);
	}

	public void ClearConversation(string userId)
	{
		_logger.LogInformation("Clearing conversation for user {UserId}", userId);

		_conversations.TryRemove(userId, out _);
		if (_userLocks.TryRemove(userId, out var sem))
		{
			sem.Dispose();
		}

		_ = _hubContext.Clients.Group(userId).SessionCleared();
	}
}
