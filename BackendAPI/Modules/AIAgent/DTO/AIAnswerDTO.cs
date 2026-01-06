namespace AIAgent.DTO;

public record AIAnswerDTO(
    List<string> Answers,
    string? DownloadUrl = null
);

