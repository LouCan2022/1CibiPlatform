namespace ATS.Data.Repository.AuditTrail;

// Deliberately NOT cached, and no ATSCacheRepository decorator: the trail is append-only
// and the whole point of the screen is to show what just happened, so a cached first page
// would hide the most recent action. Same reasoning as OMSTicketingRepository.
public sealed class AtsAuditRepository : IAtsAuditRepository
{
	private readonly ATSDBContext _dbContext;

	public AtsAuditRepository(ATSDBContext dbContext) => _dbContext = dbContext;

	public async Task<List<AuditTrailListDTO>> GetAuditTrailPageAsync(
		DateTime? afterOccurredAt,
		Guid? afterEntryId,
		int take,
		string? outcome,
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken)
	{
		var query = BuildRowsQuery(outcome, action, area, searchTerm, startDate, endDate);

		if (afterOccurredAt.HasValue && afterEntryId.HasValue)
		{
			query = ApplySeek(query, afterOccurredAt.Value, afterEntryId.Value);
		}

		return await ApplyOrder(query)
			.Take(take)
			.Select(Projection)
			.ToListAsync(cancellationToken);
	}

	public Task<long> CountAuditTrailAsync(
		string? outcome,
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken) =>
		BuildRowsQuery(outcome, action, area, searchTerm, startDate, endDate)
			.LongCountAsync(cancellationToken);

	// One round trip for both buckets. The outcome filter is deliberately not applied:
	// the chips must keep showing every bucket's size while one of them is selected.
	public async Task<AuditOutcomeCountsDTO> GetOutcomeCountsAsync(
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken)
	{
		var grouped = await BuildRowsQuery(
				outcome: null,
				action,
				area,
				searchTerm,
				startDate,
				endDate)
			.GroupBy(entry => entry.Outcome)
			.Select(group => new OutcomeCountRow
			{
				Outcome = group.Key,
				Count = group.LongCount()
			})
			.ToListAsync(cancellationToken);

		return new AuditOutcomeCountsDTO
		{
			Success = CountFor(grouped, AuditOutcome.Success),
			Failure = CountFor(grouped, AuditOutcome.Failure),

			// Every row, including any outcome outside the known vocabulary, so the "All"
			// chip never silently under-reports.
			Total = grouped.Sum(entry => entry.Count)
		};

		static long CountFor(List<OutcomeCountRow> grouped, string outcome) =>
			grouped
				.Where(entry => entry.Outcome == outcome)
				.Select(entry => entry.Count)
				.FirstOrDefault();
	}

	private IQueryable<AtsAuditEntry> BuildRowsQuery(
		string? outcome,
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate)
	{
		var query = _dbContext.AuditTrail.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(outcome))
		{
			query = query.Where(entry => entry.Outcome == outcome);
		}

		if (!string.IsNullOrWhiteSpace(action))
		{
			query = query.Where(entry => entry.Action == action);
		}

		if (!string.IsNullOrWhiteSpace(area))
		{
			query = query.Where(entry => entry.Area == area);
		}

		if (startDate.HasValue)
		{
			var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
			query = query.Where(entry => entry.OccurredAt >= start);
		}

		if (endDate.HasValue)
		{
			var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
			query = query.Where(entry => entry.OccurredAt < end);
		}

		if (!string.IsNullOrWhiteSpace(searchTerm))
		{
			var search = $"%{searchTerm.Trim()}%";

			// Who and what, not the payload: an ILIKE over a jsonb column would not use an
			// index and would let a search term reach masked content. Site is included so
			// the trail can be narrowed to one office.
			query = query.Where(entry =>
				EF.Functions.ILike(entry.UserFullName ?? string.Empty, search)
				|| EF.Functions.ILike(entry.UserEmail ?? string.Empty, search)
				|| EF.Functions.ILike(entry.Site ?? string.Empty, search)
				|| EF.Functions.ILike(entry.Action, search)
				|| EF.Functions.ILike(entry.Area, search));
		}

		return query;
	}

	// Newest first, unique AuditEntryId as the tiebreaker. ApplySeek must mirror this
	// expression exactly. Matches IX (OccurredAt DESC, AuditEntryId DESC).
	private static IQueryable<AtsAuditEntry> ApplyOrder(IQueryable<AtsAuditEntry> query) =>
		query
			.OrderByDescending(entry => entry.OccurredAt)
			.ThenByDescending(entry => entry.AuditEntryId);

	private static IQueryable<AtsAuditEntry> ApplySeek(
		IQueryable<AtsAuditEntry> query,
		DateTime afterOccurredAt,
		Guid afterEntryId) =>
		query.Where(entry => entry.OccurredAt < afterOccurredAt
			|| (entry.OccurredAt == afterOccurredAt
				&& entry.AuditEntryId.CompareTo(afterEntryId) < 0));

	private static readonly Expression<Func<AtsAuditEntry, AuditTrailListDTO>> Projection =
		entry => new AuditTrailListDTO
		{
			AuditEntryId = entry.AuditEntryId,
			OccurredAt = entry.OccurredAt,
			Action = entry.Action,
			Area = entry.Area,
			Outcome = entry.Outcome,
			FailureReason = entry.FailureReason,
			DurationMs = entry.DurationMs,
			UserId = entry.UserId,
			UserEmail = entry.UserEmail,
			UserFullName = entry.UserFullName,
			AtsRoleId = entry.AtsRoleId,
			AtsClientId = entry.AtsClientId,
			Site = entry.Site,
			IsPlatformSuperAdmin = entry.IsPlatformSuperAdmin,
			IpAddress = entry.IpAddress,
			TraceId = entry.TraceId,
			Payload = entry.Payload,
			Changes = entry.Changes
		};

	private sealed class OutcomeCountRow
	{
		public string? Outcome { get; set; }

		public long Count { get; set; }
	}
}
