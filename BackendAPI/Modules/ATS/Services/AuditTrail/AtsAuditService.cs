namespace ATS.Services.AuditTrail;

public sealed class AtsAuditService : IAtsAuditService
{
	private readonly IAtsAuditRepository _auditRepository;
	private readonly ICurrentUser _currentUser;
	private readonly ILogger<AtsAuditService> _logger;

	public AtsAuditService(
		IAtsAuditRepository auditRepository,
		ICurrentUser currentUser,
		ILogger<AtsAuditService> logger)
	{
		_auditRepository = auditRepository;
		_currentUser = currentUser;
		_logger = logger;
	}

	public async Task<KeysetPaginatedResult<AuditTrailListDTO>> GetAuditTrailAsync(
		KeysetPaginationRequest paginationRequest,
		string? outcome,
		string? action,
		string? area,
		CancellationToken cancellationToken)
	{
		if (!CanRead())
		{
			return new KeysetPaginatedResult<AuditTrailListDTO>(
				Array.Empty<AuditTrailListDTO>(),
				null,
				0);
		}

		var normalizedOutcome = NormalizeOutcome(outcome);

		// Cursor over the fixed (OccurredAt DESC, AuditEntryId DESC) ordering. An
		// undecodable cursor (malformed, stale) means "first page".
		var fields = CursorCodec.Decode(paginationRequest.Cursor, 2);

		DateTime? afterOccurredAt = DateTime.TryParse(
			fields?[0],
			CultureInfo.InvariantCulture,
			DateTimeStyles.RoundtripKind,
			out var occurredAt)
			? occurredAt
			: null;

		Guid? afterEntryId = Guid.TryParse(fields?[1], out var entryId)
			? entryId
			: null;

		var hasSeek = afterOccurredAt.HasValue && afterEntryId.HasValue;
		var pageSize = KeysetPage.Clamp(paginationRequest.PageSize);

		var rows = await _auditRepository.GetAuditTrailPageAsync(
			hasSeek ? afterOccurredAt : null,
			hasSeek ? afterEntryId : null,
			pageSize + 1,
			normalizedOutcome,
			action,
			area,
			paginationRequest.SearchTerm,
			paginationRequest.StartDate,
			paginationRequest.EndDate,
			cancellationToken);

		var (page, hasMore) = KeysetPage.Trim(rows, pageSize);

		var nextCursor = hasMore
			? CursorCodec.Encode(
				page[^1].OccurredAt.ToString("O", CultureInfo.InvariantCulture),
				page[^1].AuditEntryId.ToString("D"))
			: null;

		long? totalCount = hasSeek
			? null
			: await _auditRepository.CountAuditTrailAsync(
				normalizedOutcome,
				action,
				area,
				paginationRequest.SearchTerm,
				paginationRequest.StartDate,
				paginationRequest.EndDate,
				cancellationToken);

		return new KeysetPaginatedResult<AuditTrailListDTO>(page, nextCursor, totalCount);
	}

	public async Task<AuditOutcomeCountsDTO> GetOutcomeCountsAsync(
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken)
	{
		if (!CanRead())
		{
			return new AuditOutcomeCountsDTO();
		}

		return await _auditRepository.GetOutcomeCountsAsync(
			action,
			area,
			searchTerm,
			startDate,
			endDate,
			cancellationToken);
	}

	// Super admin only, and deliberately not IAtsAccessScopeResolver: this screen is not
	// client-scoped, because a trail the audited user can read is a weaker control. A
	// caller without the right reads an empty list rather than a 403, which is how every
	// other ATS list behaves.
	private bool CanRead()
	{
		if (_currentUser.IsAuthenticated && _currentUser.IsPlatformSuperAdmin)
		{
			return true;
		}

		_logger.LogWarning(
			"Audit trail read denied for user {UserId}: platform super admin is required",
			_currentUser.UserId);

		return false;
	}

	// An unrecognised outcome would otherwise reach the repository as a literal filter and
	// silently return nothing; treat it as "no filter" instead.
	private static string? NormalizeOutcome(string? outcome) =>
		!string.IsNullOrWhiteSpace(outcome)
			&& AuditOutcome.All.FirstOrDefault(known =>
				string.Equals(known, outcome.Trim(), StringComparison.OrdinalIgnoreCase)) is { } matched
			? matched
			: null;
}
