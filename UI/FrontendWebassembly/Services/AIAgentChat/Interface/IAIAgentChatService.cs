namespace FrontendWebassembly.Services.AIAgentChat.Interface;

public interface IAIAgentChatService
{
	// Events raised by the implementation when SignalR messages arrive
	event Action<string> AiResponseReceived;
	event Action<bool> TypingStatusChanged;

	Task<AIAnswerDTO> AskAIAsync(
		string question,
		CancellationToken cancellationToken);
}

