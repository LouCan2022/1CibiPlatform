namespace FrontendWebassembly.Services.ATS.Notifications;

/// <summary>
/// The ATS notification inbox: the live hub connection, the unread badge, and the reads
/// the bell and the notifications page are built on.
/// </summary>
public interface INotificationService : IAsyncDisposable
{
	/// <summary>Unread count for the badge. Kept current by the hub after the first load.</summary>
	long UnreadCount { get; }

	/// <summary>Raised when a notification arrives or the unread count changes.</summary>
	event Action<NotificationDTO?>? NotificationsChanged;

	/// <summary>
	/// Opens the hub connection and seeds the unread count. Idempotent - the layout calls
	/// it on every ATS page and only the first call does any work.
	/// </summary>
	Task StartAsync();

	Task<ServiceResponse<KeysetPaginatedResult<NotificationDTO>>> GetNotificationsAsync(
		string? cursor = null,
		int pageSize = 15,
		bool unreadOnly = false);

	Task<ServiceResponse<long>> RefreshUnreadCountAsync();

	Task<ServiceResponse<bool>> MarkAsReadAsync(Guid notificationId);

	Task<ServiceResponse<int>> MarkAllAsReadAsync();
}
