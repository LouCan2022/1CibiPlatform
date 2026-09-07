using ATS.Configuration;

namespace ATS.BackgroundJobs.AuditTrail;

/// <summary>
/// Deletes audit entries past the retention window. Modelled on
/// PlatformLogRetentionService, which solves the same problem for platform logs.
/// </summary>
public sealed class AtsAuditRetentionService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly AtsAuditOptions _options;
	private readonly ILogger<AtsAuditRetentionService> _logger;

	public AtsAuditRetentionService(
		IServiceScopeFactory scopeFactory,
		IOptions<AtsAuditOptions> options,
		ILogger<AtsAuditRetentionService> logger)
	{
		_scopeFactory = scopeFactory;
		_options = options.Value;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_options.Enabled || !_options.RetentionEnabled)
		{
			return;
		}

		var retentionInterval = TimeSpan.FromHours(
			Math.Max(1, _options.RetentionIntervalHours));

		using var timer = new PeriodicTimer(retentionInterval);

		do
		{
			try
			{
				await SweepAsync(stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "ATS audit trail retention failed");
			}
		}
		while (await timer.WaitForNextTickAsync(stoppingToken));
	}

	// Deletes in batches until a pass comes back short, so one sweep can clear a large
	// backlog without holding a single enormous DELETE open.
	private async Task SweepAsync(CancellationToken cancellationToken)
	{
		var batchSize = Math.Max(100, _options.RetentionBatchSize);
		var cutoff = DateTime.UtcNow.AddDays(-Math.Max(1, _options.RetentionDays));

		int deleted;

		do
		{
			using var scope = _scopeFactory.CreateScope();

			var dbContext = scope.ServiceProvider.GetRequiredService<ATSDBContext>();

			var expiredIds = dbContext.AuditTrail
				.Where(entry => entry.OccurredAt < cutoff)
				.OrderBy(entry => entry.OccurredAt)
				.Select(entry => entry.AuditEntryId)
				.Take(batchSize);

			deleted = await dbContext.AuditTrail
				.Where(entry => expiredIds.Contains(entry.AuditEntryId))
				.ExecuteDeleteAsync(cancellationToken);

			if (deleted > 0)
			{
				_logger.LogInformation(
					"Deleted {Count} ATS audit entries older than {Cutoff}",
					deleted,
					cutoff);
			}
		}
		while (deleted >= batchSize && !cancellationToken.IsCancellationRequested);
	}
}
