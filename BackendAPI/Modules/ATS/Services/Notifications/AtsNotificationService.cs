namespace ATS.Services.Notifications;

public sealed class AtsNotificationService : IAtsNotificationService
{
	private readonly IAtsNotificationRepository _notificationRepository;
	private readonly IHubContext<ATSHub, IATSClient> _hubContext;
	private readonly ILogger<AtsNotificationService> _logger;

	public AtsNotificationService(
		IAtsNotificationRepository notificationRepository,
		IHubContext<ATSHub, IATSClient> hubContext,
		ILogger<AtsNotificationService> logger)
	{
		_notificationRepository = notificationRepository;
		_hubContext = hubContext;
		_logger = logger;
	}

	public async Task RaiseAsync(
		Guid recipientUserId,
		string type,
		string title,
		string body,
		string? linkUrl,
		Guid? entityId,
		CancellationToken cancellationToken)
	{
		// No recipient means nobody to tell. Happens for orders placed through the public
		// API, which have no ATS user behind them.
		if (recipientUserId == Guid.Empty)
		{
			return;
		}

		var notification = new AtsNotification
		{
			// Version 7 so rows sort by creation even when two land in the same tick, which
			// is what makes the keyset tie-breaker stable.
			NotificationId = Guid.CreateVersion7(),
			RecipientUserId = recipientUserId,
			Type = type,
			Title = Truncate(title, TitleMaxLength) ?? string.Empty,
			Body = Truncate(body, BodyMaxLength) ?? string.Empty,
			LinkUrl = Truncate(linkUrl, LinkUrlMaxLength),
			EntityId = entityId,
			IsRead = false,
			CreatedAt = DateTime.UtcNow
		};

		// Persist first, push second. A push reaches only a live connection; the row is
		// what makes the notification survive the recipient being logged out, and it is
		// also what the badge counts.
		//
		// Both steps go through SideEffectGuard rather than being allowed to throw: every
		// caller is finishing work that has already committed, so letting an exception
		// reach CustomExceptionHandler would answer a successful submission with a 500.
		var persisted = false;

		await SideEffectGuard.RunAsync(
			async () =>
			{
				await _notificationRepository.AddAsync(notification, cancellationToken);
				persisted = true;
			},
			_logger,
			$"persist notification {type} for {recipientUserId}",
			cancellationToken);

		// Nothing stored means nothing to announce - the recipient would see a toast for a
		// notification that is not in their inbox.
		if (!persisted)
		{
			return;
		}

		await SideEffectGuard.RunAsync(
			() => _hubContext
				.Clients
				.Group(recipientUserId.ToString())
				.ReceiveNotification(ToDto(notification)),
			_logger,
			$"push notification {notification.NotificationId}",
			cancellationToken);
	}

	public async Task RaiseForOrderAsync(
		Guid emailInvitationId,
		string type,
		CancellationToken cancellationToken)
	{
		// Same contract as RaiseAsync: the caller has already finished its real work, so a
		// failed lookup degrades to "no notification" rather than failing the request.
		var target = await SideEffectGuard.RunAsync(
			() => _notificationRepository.GetOrderTargetAsync(emailInvitationId, cancellationToken),
			_logger,
			$"resolve notification target for order {emailInvitationId}",
			cancellationToken: cancellationToken);

		// No order, or an order nobody in ATS placed (public API). Nothing to tell anyone.
		if (target?.RequestorId is null || target.RequestorId == Guid.Empty)
		{
			return;
		}

		var subjectName = string.IsNullOrWhiteSpace(target.SubjectName)
			? "A candidate"
			: target.SubjectName;

		var (title, body) = BuildOrderMessage(type, subjectName);

		await RaiseAsync(
			target.RequestorId.Value,
			type,
			title,
			body,
			BuildOrderLink(type, target.SubjectName, target.LastName),
			emailInvitationId,
			cancellationToken);
	}

	private static (string Title, string Body) BuildOrderMessage(string type, string subjectName) =>
		type switch
		{
			AtsNotificationType.ApplicationFormSubmitted => (
				"Application form submitted",
				$"{subjectName} completed the application form you sent. The order is now in progress."),

			AtsNotificationType.OrderCompleted => (
				"Order completed",
				$"The order for {subjectName} is complete."),

			AtsNotificationType.ReportReady => (
				"Report ready",
				$"A report for {subjectName} has been uploaded and is ready to view."),

			AtsNotificationType.OrderDisputed => (
				"Order disputed",
				$"The order for {subjectName} was marked as disputed and needs review."),

			AtsNotificationType.TicketingFailed => (
				"Ticketing failed",
				$"Automatic OMS ticketing for {subjectName} ran out of retries. Use Retry on the ticketing board to queue it again."),

			AtsNotificationType.InvitationEmailFailed => (
				"Invitation email failed",
				$"The application form invitation for {subjectName} could not be delivered."),

			_ => (
				"Order update",
				$"There is an update on the order for {subjectName}.")
		};

	// Ticketing failures belong on the ticketing board; everything else is an order, so it
	// opens Orders & Reports. Both are pre-filtered to the subject so the reader lands on
	// the row the notification is about rather than the top of a list.
	//
	// The two boards search differently, and the search term has to match the destination:
	//
	//   Orders & Reports  ILIKE over (FirstName || ' ' || LastName)  -> full name works
	//   Ticketing Status  ILIKE FirstName OR ILIKE LastName          -> full name matches
	//                                                                  NEITHER column
	//
	// So ticketing gets the last name alone. Sending "Russel Gutierrez" there returns an
	// empty board, which reads as a broken link.
	private static string? BuildOrderLink(
		string type,
		string subjectName,
		string? lastName)
	{
		if (type == AtsNotificationType.TicketingFailed)
		{
			return string.IsNullOrWhiteSpace(lastName)
				? "/s&i/ats/ticketingstatus"
				: $"/s&i/ats/ticketingstatus?search={Uri.EscapeDataString(lastName)}";
		}

		return string.IsNullOrWhiteSpace(subjectName)
			? "/s&i/ats/searchreport"
			: $"/s&i/ats/searchreport?search={Uri.EscapeDataString(subjectName)}";
	}

	public async Task RaiseForCompletedBulkEmailsAsync(
		IReadOnlyCollection<Guid> sentEmailInvitationIds,
		CancellationToken cancellationToken)
	{
		if (sentEmailInvitationIds.Count == 0)
		{
			return;
		}

		var completedFiles = await SideEffectGuard.RunAsync(
			() => _notificationRepository.GetCompletedBulkEmailFilesAsync(
				sentEmailInvitationIds,
				cancellationToken),
			_logger,
			"resolve bulk files whose invitation emails just completed",
			fallback: [],
			cancellationToken);

		foreach (var file in completedFiles ?? [])
		{
			if (file.UploadedByUserId is not Guid uploaderId)
			{
				// Uploaded through the public API - no ATS user to tell.
				continue;
			}

			var fileLabel = string.IsNullOrWhiteSpace(file.FileName)
				? "Your bulk upload"
				: $"\"{file.FileName}\"";

			// The counts are the message. "40/40" is what the uploader is waiting to see;
			// a partial result names the failures so they can be resent.
			var body = file.FailedCount == 0
				? $"All {file.SentCount} of {file.TotalCount} invitation emails for {fileLabel} have been sent."
				: $"{file.SentCount} of {file.TotalCount} invitation emails for {fileLabel} were sent. {file.FailedCount} could not be delivered and can be resent.";

			var title = file.FailedCount == 0
				? "Invitations sent"
				: "Invitations sent with errors";

			await RaiseAsync(
				uploaderId,
				AtsNotificationType.BulkEmailsCompleted,
				title,
				body,
				string.IsNullOrWhiteSpace(file.FileName)
					? "/s&i/ats/bulkuploads"
					: $"/s&i/ats/bulkuploads?search={Uri.EscapeDataString(file.FileName)}",
				file.FileId,
				cancellationToken);
		}
	}

	public async Task<KeysetPaginatedResult<NotificationListDTO>> GetNotificationsAsync(
		Guid recipientUserId,
		KeysetPaginationRequest request,
		bool unreadOnly,
		CancellationToken cancellationToken)
	{
		// Cursor over the fixed (CreatedAt DESC, NotificationId DESC) ordering. An
		// undecodable cursor (malformed, stale) means "first page".
		var fields = CursorCodec.Decode(request.Cursor, 2);

		DateTime? afterCreatedAt = DateTime.TryParse(
			fields?[0],
			CultureInfo.InvariantCulture,
			DateTimeStyles.RoundtripKind,
			out var createdAt)
			? createdAt
			: null;

		Guid? afterNotificationId = Guid.TryParse(fields?[1], out var notificationId)
			? notificationId
			: null;

		var hasSeek = afterCreatedAt.HasValue && afterNotificationId.HasValue;
		var pageSize = KeysetPage.Clamp(request.PageSize);

		var rows = await _notificationRepository.GetNotificationsPageAsync(
			recipientUserId,
			hasSeek ? afterCreatedAt : null,
			hasSeek ? afterNotificationId : null,
			pageSize + 1,
			unreadOnly,
			cancellationToken);

		var (page, hasMore) = KeysetPage.Trim(rows, pageSize);

		var nextCursor = hasMore
			? CursorCodec.Encode(
				page[^1].CreatedAt.ToString("O", CultureInfo.InvariantCulture),
				page[^1].NotificationId.ToString("D"))
			: null;

		long? totalCount = hasSeek
			? null
			: await _notificationRepository.CountNotificationsAsync(
				recipientUserId,
				unreadOnly,
				cancellationToken);

		return new KeysetPaginatedResult<NotificationListDTO>(page, nextCursor, totalCount);
	}

	public async Task<NotificationUnreadCountDTO> GetUnreadCountAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken) =>
		new()
		{
			UnreadCount = await _notificationRepository.GetUnreadCountAsync(
				recipientUserId,
				cancellationToken)
		};

	public Task<bool> MarkAsReadAsync(
		Guid recipientUserId,
		Guid notificationId,
		CancellationToken cancellationToken) =>
		_notificationRepository.MarkAsReadAsync(recipientUserId, notificationId, cancellationToken);

	public Task<int> MarkAllAsReadAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken) =>
		_notificationRepository.MarkAllAsReadAsync(recipientUserId, cancellationToken);

	private const int TitleMaxLength = 160;
	private const int BodyMaxLength = 500;
	private const int LinkUrlMaxLength = 500;

	// The column widths are the contract. A subject with an unusually long name must not
	// turn a successful order into a failed insert.
	private static string? Truncate(string? value, int maxLength) =>
		string.IsNullOrEmpty(value) || value.Length <= maxLength
			? value
			: value[..maxLength];

	private static NotificationListDTO ToDto(AtsNotification notification) =>
		new()
		{
			NotificationId = notification.NotificationId,
			CreatedAt = notification.CreatedAt,
			Type = notification.Type,
			Title = notification.Title,
			Body = notification.Body,
			LinkUrl = notification.LinkUrl,
			EntityId = notification.EntityId,
			IsRead = notification.IsRead
		};
}
