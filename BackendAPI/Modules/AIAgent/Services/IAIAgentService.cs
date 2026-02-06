namespace AIAgent.Services;

public interface IAIAgentService
{
	Task<AIAnswerDTO> GetAIAnswerAsync(
		string userId,
		string question,
		UploadedFileDto? uploadedFile,
		string? explicitSkillName,
		CancellationToken cancellationToken);

	void ClearConversation(string userId);
}
