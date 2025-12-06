namespace FrontendWebassembly.DTO.AIAgentChat;

public record AIAnswerDTO(
	List<string> Answers,
	string? DownloadUrl = null,
	string? ErrorMessage = null
);