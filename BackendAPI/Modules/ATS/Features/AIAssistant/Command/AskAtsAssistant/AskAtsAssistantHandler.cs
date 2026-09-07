namespace ATS.Features.AIAssistant.Command.AskAtsAssistant;

// Not audited: this is a conversational turn, not a state change. The question text would
// bury the trail in noise. The one assistant action that does change state -
// ConfirmOrderDraftCommand, which creates a real order - is audited like any other write.
[SkipAudit]
public record AskAtsAssistantCommand(string Question) : ICommand<AskAtsAssistantResult>;

public record AskAtsAssistantResult(AtsChatAnswerDTO Answer);

public class AskAtsAssistantCommandValidator : AbstractValidator<AskAtsAssistantCommand>
{
	public AskAtsAssistantCommandValidator()
	{
		RuleFor(x => x.Question)
			.NotEmpty()
			.WithMessage("A question is required.")
			.MaximumLength(2000)
			.WithMessage("The question must not exceed 2000 characters.");
	}
}

public class AskAtsAssistantHandler : ICommandHandler<AskAtsAssistantCommand, AskAtsAssistantResult>
{
	private readonly IAtsAssistantService _assistantService;

	public AskAtsAssistantHandler(IAtsAssistantService assistantService)
	{
		_assistantService = assistantService;
	}

	public async Task<AskAtsAssistantResult> Handle(
		AskAtsAssistantCommand request,
		CancellationToken cancellationToken)
	{
		var answer = await _assistantService.AskAsync(request.Question, cancellationToken);

		return new AskAtsAssistantResult(answer);
	}
}
