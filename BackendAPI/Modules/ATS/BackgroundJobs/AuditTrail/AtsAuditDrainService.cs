namespace ATS.BackgroundJobs.AuditTrail;

/// <summary>
/// Drains queued audit entries into the database in batches.
/// A BackgroundService rather than a Quartz job: the other ATS jobs tick on a schedule to
/// find work already sitting in the database, whereas this is a continuous consumer of an
/// in-memory queue and should write as soon as there is something to write.
/// </summary>
public sealed class AtsAuditDrainService : BackgroundService
{
	private readonly AtsAuditWriter _auditWriter;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly AtsAuditOptions _options;
	private readonly ILogger<AtsAuditDrainService> _logger;

	// Takes the concrete writer, not IAtsAuditWriter: the interface deliberately exposes
	// only the enqueue side, so nothing running in a request scope can consume the queue.
	public AtsAuditDrainService(
		AtsAuditWriter auditWriter,
		IServiceScopeFactory scopeFactory,
		IOptions<AtsAuditOptions> options,
		ILogger<AtsAuditDrainService> logger)
	{
		_auditWriter = auditWriter;
		_scopeFactory = scopeFactory;
		_options = options.Value;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_options.Enabled)
		{
			return;
		}

		var batchSize = Math.Max(1, _options.BatchSize);
		var batch = new List<AtsAuditEntry>(batchSize);
		var reader = _auditWriter.Reader;

		try
		{
			while (await reader.WaitToReadAsync(stoppingToken))
			{
				batch.Clear();

				while (batch.Count < batchSize && reader.TryRead(out var entry))
				{
					batch.Add(entry);
				}

				if (batch.Count == 0)
				{
					continue;
				}

				await WriteBatchAsync(batch, stoppingToken);
			}
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			// Shutting down.
		}
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		// Closing the queue lets WaitToReadAsync return false once the backlog is drained,
		// so entries already recorded are not lost on a graceful shutdown.
		_auditWriter.Complete();

		await base.StopAsync(cancellationToken);
	}

	private async Task WriteBatchAsync(
		List<AtsAuditEntry> batch,
		CancellationToken cancellationToken)
	{
		try
		{
			using var scope = _scopeFactory.CreateScope();

			var dbContext = scope.ServiceProvider.GetRequiredService<ATSDBContext>();

			await ResolveSitesAsync(dbContext, batch, cancellationToken);

			dbContext.AuditTrail.AddRange(batch);

			await dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception exception)
		{
			// Not retried: a failing batch retried forever would block every entry behind
			// it, and the loop must keep running whatever one batch does.
			_logger.LogError(
				exception,
				"Failed to persist {Count} ATS audit entries",
				batch.Count);
		}
	}

	/// <summary>
	/// Fills in each entry's Site from UserDetails.
	/// Site is the one recorded field that is not a claim, so it cannot be read in the
	/// request the way the role and client are. Resolving it here costs one query per
	/// batch instead of a round trip on every audited request, which is the whole reason
	/// the write was moved off the request thread.
	/// Like the rest of the caller's details it is then denormalised onto the row, so a
	/// later transfer to another site does not rewrite history.
	/// </summary>
	private static async Task ResolveSitesAsync(
		ATSDBContext dbContext,
		List<AtsAuditEntry> batch,
		CancellationToken cancellationToken)
	{
		var userIds = batch
			.Where(entry => entry.UserId.HasValue)
			.Select(entry => entry.UserId!.Value)
			.Distinct()
			.ToArray();

		if (userIds.Length == 0)
		{
			return;
		}

		// UserDetails is keyed (UserId, ModuleId): one row per module grant, each carrying
		// the same Site. Grouped so any one of them answers for the user, the same
		// assumption OMSTicketingRepository's Take(1) makes.
		var sitesByUser = await dbContext.UserDetails
			.AsNoTracking()
			.Where(user => userIds.Contains(user.UserId))
			.GroupBy(user => user.UserId)
			.Select(group => new
			{
				UserId = group.Key,
				Site = group.Select(user => user.Site).FirstOrDefault()
			})
			.ToDictionaryAsync(row => row.UserId, row => row.Site, cancellationToken);

		foreach (var entry in batch)
		{
			// A user with no ATS UserDetails row - a platform super admin who was never
			// granted a module - simply has no site, and the entry still stands.
			if (entry.UserId is { } userId && sitesByUser.TryGetValue(userId, out var site))
			{
				entry.Site = site;
			}
		}
	}
}
