namespace ATS.Data.Repository.Notifications;

// Deliberately NOT cached, and no ATSCacheRepository decorator: the whole point of the
// bell is to show what just happened, so a cached unread count or first page would hide
// the notification that was raised a second ago. Same reasoning as AtsAuditRepository and
// OMSTicketingRepository.
public sealed class AtsNotificationRepository : IAtsNotificationRepository
{
	private readonly ATSDBContext _dbContext;

	public AtsNotificationRepository(ATSDBContext dbContext) => _dbContext = dbContext;

	public async Task AddAsync(AtsNotification notification, CancellationToken cancellationToken)
	{
		_dbContext.Notifications.Add(notification);

		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task<List<NotificationListDTO>> GetNotificationsPageAsync(
		Guid recipientUserId,
		DateTime? afterCreatedAt,
		Guid? afterNotificationId,
		int take,
		bool unreadOnly,
		CancellationToken cancellationToken)
	{
		var query = BuildRowsQuery(recipientUserId, unreadOnly);

		if (afterCreatedAt.HasValue && afterNotificationId.HasValue)
		{
			query = ApplySeek(query, afterCreatedAt.Value, afterNotificationId.Value);
		}

		return await ApplyOrder(query)
			.Take(take)
			.Select(Projection)
			.ToListAsync(cancellationToken);
	}

	public Task<long> CountNotificationsAsync(
		Guid recipientUserId,
		bool unreadOnly,
		CancellationToken cancellationToken) =>
		BuildRowsQuery(recipientUserId, unreadOnly)
			.LongCountAsync(cancellationToken);

	public Task<long> GetUnreadCountAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken) =>
		_dbContext.Notifications
			.AsNoTracking()
			.Where(notification => notification.RecipientUserId == recipientUserId
				&& !notification.IsRead)
			.LongCountAsync(cancellationToken);

	// The recipient predicate is part of the UPDATE rather than a lookup-then-check, so
	// another user's id simply matches no rows instead of being fetched and rejected.
	public async Task<bool> MarkAsReadAsync(
		Guid recipientUserId,
		Guid notificationId,
		CancellationToken cancellationToken)
	{
		var updated = await _dbContext.Notifications
			.Where(notification => notification.NotificationId == notificationId
				&& notification.RecipientUserId == recipientUserId
				&& !notification.IsRead)
			.ExecuteUpdateAsync(
				setters => setters
					.SetProperty(notification => notification.IsRead, true)
					.SetProperty(notification => notification.ReadAt, DateTime.UtcNow),
				cancellationToken);

		return updated > 0;
	}

	public Task<int> MarkAllAsReadAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken) =>
		_dbContext.Notifications
			.Where(notification => notification.RecipientUserId == recipientUserId
				&& !notification.IsRead)
			.ExecuteUpdateAsync(
				setters => setters
					.SetProperty(notification => notification.IsRead, true)
					.SetProperty(notification => notification.ReadAt, DateTime.UtcNow),
				cancellationToken);

	public Task<NotificationOrderTargetDTO?> GetOrderTargetAsync(
		Guid emailInvitationId,
		CancellationToken cancellationToken) =>
		_dbContext.EmailInvitationRequests
			.AsNoTracking()
			.Where(order => order.EmailInvitationID == emailInvitationId)
			.Select(order => new NotificationOrderTargetDTO
			{
				EmailInvitationId = order.EmailInvitationID,
				RequestorId = order.RequestorId,
				FirstName = order.FirstName,
				LastName = order.LastName
			})
			.FirstOrDefaultAsync(cancellationToken);

	private IQueryable<AtsNotification> BuildRowsQuery(Guid recipientUserId, bool unreadOnly)
	{
		var query = _dbContext.Notifications
			.AsNoTracking()
			.Where(notification => notification.RecipientUserId == recipientUserId);

		if (unreadOnly)
		{
			query = query.Where(notification => !notification.IsRead);
		}

		return query;
	}

	// Newest first, unique NotificationId as the tiebreaker. ApplySeek must mirror this
	// expression exactly. Matches IX (RecipientUserId, CreatedAt DESC, NotificationId DESC).
	private static IQueryable<AtsNotification> ApplyOrder(IQueryable<AtsNotification> query) =>
		query
			.OrderByDescending(notification => notification.CreatedAt)
			.ThenByDescending(notification => notification.NotificationId);

	private static IQueryable<AtsNotification> ApplySeek(
		IQueryable<AtsNotification> query,
		DateTime afterCreatedAt,
		Guid afterNotificationId) =>
		query.Where(notification => notification.CreatedAt < afterCreatedAt
			|| (notification.CreatedAt == afterCreatedAt
				&& notification.NotificationId.CompareTo(afterNotificationId) < 0));

	private static readonly Expression<Func<AtsNotification, NotificationListDTO>> Projection =
		notification => new NotificationListDTO
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
