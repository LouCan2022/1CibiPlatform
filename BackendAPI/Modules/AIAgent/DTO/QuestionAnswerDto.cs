namespace AIAgent.DTO;

public record QuestionAnswerDto(
	string Question,
	string? Answer = null
);
