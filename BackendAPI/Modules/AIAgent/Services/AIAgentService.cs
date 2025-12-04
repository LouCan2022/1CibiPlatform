namespace AIAgent.Services;

public class AIAgentService : IAIAgentService
{
	private readonly Kernel _kernel;
	private readonly ILogger<AIAgentService> _logger;
	private readonly ConcurrentDictionary<string, List<(string Role, string Content)>> _conversations = new();
	private readonly ConcurrentDictionary<string, SemaphoreSlim> _userLocks = new();

	// Maximum messages to keep in memory per user (messages, not exchanges)
	private const int MaxHistoryMessages = 20;

	public AIAgentService(
		Kernel kernel,
		ILogger<AIAgentService> logger
		)
	{
		this._kernel = kernel;
		this._logger = logger;
	}

	public async Task<AIAnswerDTO> GetAIAnswerAsync(
		string userId,
		string question,
		CancellationToken cancellationToken)
	{
		var sem = _userLocks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

		await sem.WaitAsync(cancellationToken);

		try
		{
			// Retrieve or initialize conversation history
			var history = _conversations.GetOrAdd(userId, _ => new List<(string, string)>());

			// Build conversation context (no lock needed because semaphore serializes access)
			string historyText = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));

			string template = """
                  You are a helpful, honest, and concise assistant.
                  Use the conversation history below to provide context-aware answers.
                  If you don't know, say "I don't know."
                  Conversation history:
                  {{$historyText}}
                  User: {{$input}}
                  Assistant:
                  """;

			var args = new KernelArguments { ["input"] = question };
			args["historyText"] = historyText;

			_logger.LogDebug("Invoking kernel for user {UserId} with history length {HistoryCount}", userId, history.Count);

			var raw = await _kernel.InvokePromptAsync(template, args);
			var answer = raw?.ToString()?.Trim() ?? string.Empty;

			// Append new exchange to history (no lock needed because semaphore serializes access)
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
	}

}
