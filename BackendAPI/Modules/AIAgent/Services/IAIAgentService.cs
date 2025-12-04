namespace AIAgent.Services;

public interface IAIAgentService
{
	Task<AIAnswerDTO> GetAIAnswerAsync(
		string userId,
		string question,
		CancellationToken cancellationToken);

	void ClearConversation(string userId);
}
