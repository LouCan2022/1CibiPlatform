namespace AIAgent.Features.AskAI;

public record AskAIQueryRequest(
	string UserId,
	string Question,
	string ExplicitSkillName,
	UploadedFileDto? UploadedFile = null) : IQuery<AskAIQueryResult>;

public record AskAIQueryResult(AIAnswerDTO aiAnswerDTO);

public class AskAIQueryRequestValidator : AbstractValidator<AskAIQueryRequest>
{
	public AskAIQueryRequestValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty().WithMessage("UserId is required.");
		RuleFor(x => x.Question)
			.NotEmpty().WithMessage("Question is required.");
	}
}

public class AskAIHandler : IQueryHandler<AskAIQueryRequest, AskAIQueryResult>
{
	private readonly IAIAgentService _aiAgentService;

	public AskAIHandler(IAIAgentService aiAgentService)
	{
		this._aiAgentService = aiAgentService;
	}

	public async Task<AskAIQueryResult> Handle(
		AskAIQueryRequest request,
		CancellationToken cancellationToken)
	{
		var answer = await _aiAgentService.GetAIAnswerAsync(
			request.UserId,
			request.Question,
			request.UploadedFile,
			request.ExplicitSkillName,
			cancellationToken
		);

		return new AskAIQueryResult(answer);
	}
}
