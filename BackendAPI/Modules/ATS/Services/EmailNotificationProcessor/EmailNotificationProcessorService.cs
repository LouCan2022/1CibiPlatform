namespace ATS.Services.EmailNotificationProcessor;

public class EmailNotificationProcessorService : IEmailNotificationProcessorService
{
	// Atomically claims the oldest pending batch: ZPOPMIN from pending, ZADD into
	// processing with the claim timestamp as score. Atomic because Quartz runs
	// clustered, so two nodes may poll at the same time.
	private const string ClaimBatchScript = @"
		local popped = redis.call('ZPOPMIN', KEYS[1])
		if #popped == 0 then return nil end
		redis.call('ZADD', KEYS[2], ARGV[1], popped[1])
		return popped[1]";

	private readonly ILogger<EmailNotificationProcessorService> _logger;
	private readonly IATSRepository _repository;
	private readonly IAtsNotificationService _notificationService;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IConfiguration _configuration;
	private readonly string _applicationformBaseUrl;

	// How many invitations are in flight at once. The pass is almost entirely SMTP wait,
	// so sending one at a time made a 200-email pass take as long as 200 round trips.
	//
	// Bounded rather than unbounded on purpose: every concurrent send opens its own SMTP
	// connection, and a provider will throttle or block a client that opens 200 at once.
	// Matches the degree BulkSubmissionProcessorService uses for the same reason.
	private const int MaxConcurrentSends = 8;

	// Between retry attempts. Three back-to-back attempts against a server that is briefly
	// unavailable all fail for the same reason and burn the row's budget in milliseconds;
	// a short pause is what actually lets a transient fault clear.
	private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

	// Comfortably longer than a full send pass so a live worker is never robbed of rows it
	// is still processing. Worst case today is roughly four minutes: 200 messages at 8 at a
	// time is 25 batches, each up to 3 attempts with a RetryDelay between them.
	private static readonly TimeSpan StaleClaimTimeout = TimeSpan.FromMinutes(30);

	public EmailNotificationProcessorService(
		ILogger<EmailNotificationProcessorService> logger,
		IATSRepository repository,
		IAtsNotificationService notificationService,
		IServiceScopeFactory serviceScopeFactory,
		IConfiguration configuration)
	{
		_logger = logger;
		_repository = repository;
		_notificationService = notificationService;
		_serviceScopeFactory = serviceScopeFactory;
		_configuration = configuration;
		_applicationformBaseUrl = _configuration.GetSection("ATS").GetValue<string>("ApplicationFormBaseUrl") ?? string.Empty;
	}

	public async Task ProcessForPendingStatusAsync(CancellationToken cancellationToken)
	{
		// A crash mid-send leaves rows claimed as Processing with no live worker, so
		// release anything stale before claiming the next slice.
		var released = await _repository.ReleaseStaleEmailInvitationClaimsAsync(StaleClaimTimeout);

		if (released > 0)
		{
			_logger.LogWarning(
				"Released {ReleasedCount} stale email invitation claim(s) back to Pending.",
				released);
		}

		// PostgreSQL is the queue: the claim atomically moves a slice of Pending rows to
		// Processing, so a concurrent worker cannot pick up the same invitations.
		var allRequests = await _repository.GetPendingEmailInvitationRequestsAsync();

		if (allRequests.Count == 0)
		{
			return;
		}

		// Sent MaxConcurrentSends at a time rather than one after another. The pass is
		// almost entirely SMTP wait, so this is the difference between ~200 sequential
		// round trips and ~25 batches of them.
		var startedAt = DateTime.UtcNow;

		// Concurrent, not List: several sends complete at once and List<T>.Add from
		// multiple threads corrupts the backing array without throwing.
		var successBag = new ConcurrentBag<EmailInvitationRequest>();
		var errorBag = new ConcurrentBag<EmailInvitationRequest>();

		using var semaphore = new SemaphoreSlim(MaxConcurrentSends);

		var sendTasks = allRequests.Select(async request =>
		{
			await semaphore.WaitAsync(cancellationToken);

			try
			{
				if (await TrySendEmailWithRetryAsync(request, cancellationToken))
				{
					successBag.Add(request);
				}
				else
				{
					errorBag.Add(request);
				}
			}
			finally
			{
				semaphore.Release();
			}
		});

		await Task.WhenAll(sendTasks);

		var successList = successBag.ToList();
		var errorList = errorBag.ToList();

		_logger.LogInformation(
			"Email processing completed in {ElapsedSeconds:0.0}s. Success: {SuccessCount}, Failed: {FailedCount}",
			(DateTime.UtcNow - startedAt).TotalSeconds,
			successList.Count,
			errorList.Count);

		if (successList.Any())
		{
			await _repository.UpdateBulkEmailInvitationRequestForSentEmailAsync(successList);
		}

		if (errorList.Any())
		{
			await _repository.UpdateBulkEmailInvitationRequestForNotSentEmailAsync(errorList);
		}

		// After the statuses are written, so the completeness check reads the outcome of
		// this pass rather than the state before it. Every order touched is considered,
		// including the failures: a file is finished when nothing is still in flight, not
		// when everything succeeded.
		var attempted = successList
			.Concat(errorList)
			.Select(request => request.EmailInvitationID)
			.ToList();

		await _notificationService.RaiseForCompletedBulkEmailsAsync(attempted, cancellationToken);
	}


	private async Task<bool> TrySendEmailWithRetryAsync(
		EmailInvitationRequest request,
		CancellationToken cancellationToken)
	{
		const int maxAttempts = 3;

		// One scope per invitation, resolved here rather than using the injected service.
		//
		// IEndorsementSubmissionService is Scoped and reaches a DbContext (it looks up the
		// client name for the email body). DbContext is NOT thread-safe, so sharing one
		// instance across concurrent sends corrupts its change tracker in ways that surface
		// as unrelated errors much later. Each send gets its own, exactly as
		// BulkSubmissionProcessorService does for the same reason.
		using var scope = _serviceScopeFactory.CreateScope();

		var submissionService = scope.ServiceProvider
			.GetRequiredService<IEndorsementSubmissionService>();

		for (int attempt = 1; attempt <= maxAttempts; attempt++)
		{
			if (await TrySendEmailAsync(submissionService, request, attempt == 1 ? null : attempt))
			{
				return true;
			}

			// Pause before retrying, except after the final attempt - there is nothing left
			// to wait for. Without this the three attempts run in milliseconds and a server
			// that is briefly unavailable fails all of them identically.
			if (attempt < maxAttempts)
			{
				await Task.Delay(RetryDelay, cancellationToken);
			}
		}

		return false;
	}

	private async Task<bool> TrySendEmailAsync(
	IEndorsementSubmissionService submissionService,
	EmailInvitationRequest request,
	int? retry = null)
	{
		var logContext = new
		{
			Action = retry is null
				? "ApplicationFormEmailSending"
				: "RetryApplicationFormEmailSending",
			Step = "SendEmail",
			Identity = request.EmailInvitationID,
			Timestamp = DateTime.UtcNow
		};

		try
		{
			if (string.IsNullOrWhiteSpace(request.EmailAddress))
			{
				return false;
			}

			var subjectName = $"{request.FirstName} {request.LastName}";
			var applicationFormLink = $"{_applicationformBaseUrl}/{request.HashToken}";

			await submissionService.SendApplicationFormToUserEmailAsync(
				request.EmailAddress,
				subjectName,
				applicationFormLink,
				request.Requestor,
				request.ClientId);

			return true;
		}
		catch (Exception ex)
		{
			if (retry is null)
			{
				_logger.LogError(
					ex,
					"Failed to send email to {Email}: {@Context}",
					request.EmailAddress,
					logContext);
			}
			else
			{
				_logger.LogError(
					ex,
					"Retry {Retry} failed for {Email}: {@Context}",
					retry,
					request.EmailAddress,
					logContext);
			}

			return false;
		}
	}
}
