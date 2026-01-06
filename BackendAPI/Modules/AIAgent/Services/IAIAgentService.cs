using AIAgent.Features.AskAI;

namespace AIAgent.Services;

public interface IAIAgentService
{
	Task<AIAnswerDTO> GetAIAnswerAsync(
		string userId,
		string question,
		UploadedFileDto? uploadedFile,
		CancellationToken cancellationToken);

	void ClearConversation(string userId);
}
