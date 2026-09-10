namespace ATS.Services.EmailNotificationProcessor;

public class EmailNotificationProcessorService : IEmailNotificationProcessorService
{
	private readonly ILogger<EmailNotificationProcessorService> _logger;
	private readonly IATSRepository _repository;
	private readonly IAtsNotificationService _notificationService;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly IConfiguration _configuration;
	private readonly SmtpRateLimiter _rateLimiter;
	private readonly AtsEmailDeliveryOptions _options;
	private readonly string _applicationformBaseUrl;

	// Comfortably longer than a full send pass so a live worker is never robbed of rows it
	// is still processing. A pass is now bounded by the send RATE rather than by
	// concurrency: 200 messages at the default 0.9/s is roughly four minutes, and the
	// throttle back-off can add ten more.
	private static readonly TimeSpan StaleClaimTimeout = TimeSpan.FromMinutes(30);

	public EmailNotificationProcessorService(
		ILogger<EmailNotificationProcessorService> logger,
		IATSRepository repository,
		IAtsNotificationService notificationService,
		IServiceScopeFactory serviceScopeFactory,
		IConfiguration configuration,
		SmtpRateLimiter rateLimiter,
		IOptions<AtsEmailDeliveryOptions> options)
	{
		_logger = logger;
		_repository = repository;
		_notificationService = notificationService;
		_serviceScopeFactory = serviceScopeFactory;
		_configuration = configuration;
		_rateLimiter = rateLimiter;
		_options = options.Value;
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

		// A throttle from an earlier pass is still in force. Claiming rows now would only
		// park them behind the back-off while holding them out of every other worker's
		// reach; leaving them Pending costs one idle tick and nothing else.
		if (_rateLimiter.IsThrottled)
		{
			_logger.LogWarning(
				"Skipping email pass: the SMTP provider is still rate limiting this sender.");

			return;
		}

		// PostgreSQL is the queue: the claim atomically moves a slice of Pending rows to
		// Processing, so a concurrent worker cannot pick up the same invitations.
		var allRequests = await _repository.GetPendingEmailInvitationRequestsAsync();

		if (allRequests.Count == 0)
		{
			return;
		}

		var startedAt = DateTime.UtcNow;

		// Concurrent, not List: several sends complete at once and List<T>.Add from
		// multiple threads corrupts the backing array without throwing.
		var successBag = new ConcurrentBag<EmailInvitationRequest>();
		var errorBag = new ConcurrentBag<EmailInvitationRequest>();
		var abandonedBag = new ConcurrentBag<EmailInvitationRequest>();

		// Concurrency is now bounded by the connection pool and the send rate is bounded by
		// the limiter, so this only decides how many rows are in flight awaiting a slot.
		// It sits slightly above the connection count so a returning connection never waits
		// for a task to be scheduled.
		var inFlightLimit = Math.Max(1, _options.MaxConcurrentConnections * 2);

		using var semaphore = new SemaphoreSlim(inFlightLimit);

		// Trips the moment the provider says "slow down". Every task still queued checks it
		// before sending and leaves its row untouched instead of knocking again.
		using var throttleSignal = new CancellationTokenSource();

		using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
			cancellationToken,
			throttleSignal.Token);

		var sendTasks = allRequests.Select(async request =>
		{
			// Checked before the semaphore as well as inside the retry loop: a pass that is
			// already abandoning work should not queue hundreds of tasks to discover that
			// one at a time.
			if (throttleSignal.IsCancellationRequested)
			{
				abandonedBag.Add(request);
				return;
			}

			await semaphore.WaitAsync(cancellationToken);

			try
			{
				var outcome = await SendWithRetryAsync(request, linkedTokenSource.Token, throttleSignal);

				switch (outcome)
				{
					case EmailDeliveryOutcome.Sent:
						successBag.Add(request);
						break;

					// Released rather than counted as a failure. The row keeps its attempt
					// budget for a pass that runs after the throttle clears - spending it
					// against a closed door is what turned a ten-minute deferral into a
					// much longer one.
					case EmailDeliveryOutcome.Throttled:
						abandonedBag.Add(request);
						break;

					default:
						errorBag.Add(request);
						break;
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
		var abandonedList = abandonedBag.ToList();

		_logger.LogInformation(
			"Email processing completed in {ElapsedSeconds:0.0}s. Success: {SuccessCount}, Failed: {FailedCount}, Deferred: {DeferredCount}",
			(DateTime.UtcNow - startedAt).TotalSeconds,
			successList.Count,
			errorList.Count,
			abandonedList.Count);

		if (successList.Count > 0)
		{
			await _repository.UpdateBulkEmailInvitationRequestForSentEmailAsync(successList);
		}

		if (errorList.Count > 0)
		{
			await _repository.UpdateBulkEmailInvitationRequestForNotSentEmailAsync(errorList);
		}

		// Straight back to Pending with the attempt count untouched. These were never
		// offered to the provider, so charging them an attempt would retire a valid address
		// after five throttles without a single real delivery failure.
		if (abandonedList.Count > 0)
		{
			await _repository.ReleaseEmailInvitationClaimsAsync(abandonedList);

			_logger.LogWarning(
				"Deferred {DeferredCount} invitation(s) to a later pass because the provider is rate limiting.",
				abandonedList.Count);
		}

		// After the statuses are written, so the completeness check reads the outcome of
		// this pass rather than the state before it. Deferred rows are excluded on purpose:
		// they are still in flight, and a file is only finished when nothing remains.
		var attempted = successList
			.Concat(errorList)
			.Select(request => request.EmailInvitationID)
			.ToList();

		await _notificationService.RaiseForCompletedBulkEmailsAsync(attempted, cancellationToken);
	}

	/// <summary>
	/// Sends one invitation, retrying only what is worth retrying.
	///
	/// The three outcomes are the point. A permanent rejection returns immediately rather
	/// than spending three attempts on an address the server has already refused; a
	/// throttle stops the entire pass; only a genuine transient fault backs off and tries
	/// again.
	/// </summary>
	private async Task<EmailDeliveryOutcome> SendWithRetryAsync(
		EmailInvitationRequest request,
		CancellationToken cancellationToken,
		CancellationTokenSource throttleSignal)
	{
		var maxAttempts = Math.Max(1, _options.MaxAttemptsPerPass);

		for (int attempt = 1; attempt <= maxAttempts; attempt++)
		{
			if (throttleSignal.IsCancellationRequested)
			{
				return EmailDeliveryOutcome.Throttled;
			}

			var result = await TrySendEmailAsync(request, attempt == 1 ? null : attempt, cancellationToken);

			switch (result.Outcome)
			{
				case EmailDeliveryOutcome.Sent:
					return EmailDeliveryOutcome.Sent;

				case EmailDeliveryOutcome.Permanent:
					// Nothing about a second attempt changes an unknown mailbox.
					return EmailDeliveryOutcome.Permanent;

				case EmailDeliveryOutcome.Throttled:
					// Park the whole sender, then tell every queued task to stand down.
					_rateLimiter.ReportThrottled(
						TimeSpan.FromSeconds(_options.ThrottleBackoffSeconds));

					await throttleSignal.CancelAsync();

					return EmailDeliveryOutcome.Throttled;
			}

			// Exponential, not fixed: a server that is briefly unavailable needs longer than
			// two seconds, and re-knocking at a constant interval is what a provider reads
			// as a client that will not take no for an answer.
			if (attempt < maxAttempts)
			{
				var backoff = TimeSpan.FromSeconds(
					_options.RetryBaseDelaySeconds * Math.Pow(2, attempt - 1));

				await Task.Delay(backoff, cancellationToken);
			}
		}

		return EmailDeliveryOutcome.Transient;
	}

	private async Task<EmailDeliveryResult> TrySendEmailAsync(
		EmailInvitationRequest request,
		int? retry,
		CancellationToken cancellationToken)
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

		// A row with no address can never be sent, and retrying it twice more only delays
		// the pass. Permanent so it is retired rather than requeued.
		if (string.IsNullOrWhiteSpace(request.EmailAddress))
		{
			_logger.LogError(
				"Invitation has no email address and cannot be sent: {@Context}",
				logContext);

			return EmailDeliveryResult.Permanent(null, "The invitation has no email address.");
		}

		try
		{
			// One scope per attempt, resolved here rather than using an injected service.
			// IEndorsementSubmissionService is Scoped and reaches a DbContext (it looks up
			// the client name for the email body). DbContext is NOT thread-safe, so sharing
			// one instance across concurrent sends corrupts its change tracker in ways that
			// surface as unrelated errors much later.
			using var scope = _serviceScopeFactory.CreateScope();

			var submissionService = scope.ServiceProvider
				.GetRequiredService<IEndorsementSubmissionService>();

			var subjectName = $"{request.FirstName} {request.LastName}";
			var applicationFormLink = $"{_applicationformBaseUrl}/{request.HashToken}";

			return await submissionService.SendApplicationFormToUserEmailWithResultAsync(
				request.EmailAddress,
				subjectName,
				applicationFormLink,
				request.Requestor,
				request.ClientId,
				cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Shutdown, or the pass standing down after a throttle. Not a delivery failure -
			// the row keeps its budget and is picked up again later.
			return EmailDeliveryResult.Throttled(null, "The send was cancelled before it completed.");
		}
		catch (Exception ex)
		{
			// Anything the sender did not already classify: a lookup failure, a malformed
			// address, a bug. Transient is the safe reading - the row retries rather than
			// being retired on one unexplained error.
			_logger.LogError(
				ex,
				"Unclassified failure sending email to {Email} (attempt {Attempt}): {@Context}",
				request.EmailAddress,
				retry ?? 1,
				logContext);

			return EmailDeliveryResult.Transient(null, ex.Message);
		}
	}
}
