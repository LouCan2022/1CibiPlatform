namespace ATS.Features.AuditTrail.Query.GetAuditOutcomeCounts;

public record GetAuditOutcomeCountsQueryRequest(
	string? Action = null,
	string? Area = null,
	string? SearchTerm = null,
	DateTime? StartDate = null,
	DateTime? EndDate = null)
	: IQuery<GetAuditOutcomeCountsQueryResult>;

public record GetAuditOutcomeCountsQueryResult(AuditOutcomeCountsDTO Counts);

public class GetAuditOutcomeCountsQueryRequestValidator
	: AbstractValidator<GetAuditOutcomeCountsQueryRequest>
{
	public GetAuditOutcomeCountsQueryRequestValidator()
	{
		RuleFor(x => x.Action)
			.MaximumLength(120)
			.WithMessage("Action cannot exceed 120 characters.");

		RuleFor(x => x.Area)
			.MaximumLength(80)
			.WithMessage("Area cannot exceed 80 characters.");
	}
}

public class GetAuditOutcomeCountsHandler
	: IQueryHandler<GetAuditOutcomeCountsQueryRequest, GetAuditOutcomeCountsQueryResult>
{
	private readonly IAtsAuditService _auditService;

	public GetAuditOutcomeCountsHandler(IAtsAuditService auditService)
	{
		_auditService = auditService;
	}

	public async Task<GetAuditOutcomeCountsQueryResult> Handle(
		GetAuditOutcomeCountsQueryRequest request,
		CancellationToken cancellationToken)
	{
		var counts = await _auditService.GetOutcomeCountsAsync(
			request.Action,
			request.Area,
			request.SearchTerm,
			request.StartDate,
			request.EndDate,
			cancellationToken);

		return new GetAuditOutcomeCountsQueryResult(counts);
	}
}
