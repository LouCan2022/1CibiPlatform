using ATS.Configuration;

namespace ATS.BackgroundJobs.Notifications;

/// <summary>
/// Deletes in-app notifications past the retention window. Modelled on
/// AtsAuditRetentionService, which solves the same problem for the audit trail.
/// </summary>
public sealed class AtsNotificationRetentionService : BackgroundService
{
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly AtsNotificationOptions _options;
	private readonly ILogger<AtsNotificationRetentionService> _logger;

	public AtsNotificationRetentionService(
		IServiceScopeFactory scopeFactory,
		IOptions<AtsNotificationOptions> options,
		ILogger<AtsNotificationRetentionService> logger)
	{
		_scopeFactory = scopeFactory;
		_options = options.Value;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!_options.RetentionEnabled)
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
				_logger.LogError(exception, "ATS notification retention failed");
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

			var expiredIds = dbContext.Notifications
				.Where(notification => notification.CreatedAt < cutoff)
				.OrderBy(notification => notification.CreatedAt)
				.Select(notification => notification.NotificationId)
				.Take(batchSize);

			deleted = await dbContext.Notifications
				.Where(notification => expiredIds.Contains(notification.NotificationId))
				.ExecuteDeleteAsync(cancellationToken);

			if (deleted > 0)
			{
				_logger.LogInformation(
					"Deleted {Count} ATS notifications older than {Cutoff}",
					deleted,
					cutoff);
			}
		}
		while (deleted >= batchSize && !cancellationToken.IsCancellationRequested);
	}
}
