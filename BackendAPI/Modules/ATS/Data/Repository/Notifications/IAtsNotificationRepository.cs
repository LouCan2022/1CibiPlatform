namespace ATS.Data.Repository.Notifications;

/// <summary>
/// Persistence for the in-app notification inbox.
/// </summary>
/// <remarks>
/// Every read takes the recipient id as its first argument and every implementation
/// filters on it. That is the isolation boundary: a caller can only ever reach their own
/// notifications, and it is enforced here rather than being left to each handler to
/// remember.
/// </remarks>
public interface IAtsNotificationRepository
{
	Task AddAsync(AtsNotification notification, CancellationToken cancellationToken);

	Task<List<NotificationListDTO>> GetNotificationsPageAsync(
		Guid recipientUserId,
		DateTime? afterCreatedAt,
		Guid? afterNotificationId,
		int take,
		bool unreadOnly,
		CancellationToken cancellationToken);

	Task<long> CountNotificationsAsync(
		Guid recipientUserId,
		bool unreadOnly,
		CancellationToken cancellationToken);

	Task<long> GetUnreadCountAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken);

	/// <summary>Marks one notification read. Returns false when it is not the caller's.</summary>
	Task<bool> MarkAsReadAsync(
		Guid recipientUserId,
		Guid notificationId,
		CancellationToken cancellationToken);

	/// <summary>Marks every unread notification read. Returns how many changed.</summary>
	Task<int> MarkAllAsReadAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken);

	/// <summary>
	/// The requestor and subject name for one order, for addressing and wording a
	/// notification about it. Null when the order is unknown.
	/// </summary>
	/// <remarks>
	/// Lives here rather than on IApplicationFormRepository so the notification senders
	/// have one narrow projection to call instead of loading a whole EmailInvitationRequest
	/// with its seven navigation properties just to read two columns.
	/// </remarks>
	Task<NotificationOrderTargetDTO?> GetOrderTargetAsync(
		Guid emailInvitationId,
		CancellationToken cancellationToken);
}
