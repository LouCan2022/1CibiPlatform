namespace ATS.Features.AuditTrail.Query.GetAuditTrail;

public record GetAuditTrailQueryRequest(
	string? Cursor = null,
	int? PageSize = 10,
	string? Outcome = null,
	string? Action = null,
	string? Area = null,
	string? SearchTerm = null,
	DateTime? StartDate = null,
	DateTime? EndDate = null)
	: IQuery<GetAuditTrailQueryResult>;

public record GetAuditTrailQueryResult(KeysetPaginatedResult<AuditTrailListDTO> AuditEntries);

public class GetAuditTrailQueryRequestValidator : AbstractValidator<GetAuditTrailQueryRequest>
{
	public GetAuditTrailQueryRequestValidator()
	{
		RuleFor(x => x.PageSize)
			.Must(pageSize => pageSize is null || (pageSize > 0 && pageSize <= 100))
			.WithMessage("PageSize must be greater than 0 and less than or equal to 100.");

		// Cursor is deliberately unvalidated: cursors are opaque and a malformed one
		// self-heals to the first page rather than failing the request.
		RuleFor(x => x.Outcome)
			.Must(outcome => string.IsNullOrWhiteSpace(outcome)
				|| AuditOutcome.All.Contains(outcome, StringComparer.OrdinalIgnoreCase))
			.WithMessage($"Outcome must be empty or one of: {string.Join(", ", AuditOutcome.All)}.");

		RuleFor(x => x.Action)
			.MaximumLength(120)
			.WithMessage("Action cannot exceed 120 characters.");

		RuleFor(x => x.Area)
			.MaximumLength(80)
			.WithMessage("Area cannot exceed 80 characters.");
	}
}

public class GetAuditTrailHandler
	: IQueryHandler<GetAuditTrailQueryRequest, GetAuditTrailQueryResult>
{
	private readonly IAtsAuditService _auditService;

	public GetAuditTrailHandler(IAtsAuditService auditService)
	{
		_auditService = auditService;
	}

	public async Task<GetAuditTrailQueryResult> Handle(
		GetAuditTrailQueryRequest request,
		CancellationToken cancellationToken)
	{
		var paginationRequest = new KeysetPaginationRequest(
			request.Cursor,
			request.PageSize ?? 10,
			request.SearchTerm,
			request.StartDate,
			request.EndDate);

		var auditEntries = await _auditService.GetAuditTrailAsync(
			paginationRequest,
			request.Outcome,
			request.Action,
			request.Area,
			cancellationToken);

		return new GetAuditTrailQueryResult(auditEntries);
	}
}
