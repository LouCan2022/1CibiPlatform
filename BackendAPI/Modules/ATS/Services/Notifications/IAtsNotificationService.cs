namespace ATS.Services.Notifications;

/// <summary>
/// Raises and reads in-app notifications.
/// </summary>
/// <remarks>
/// The write side is called from background jobs as well as request handlers, so every
/// method takes the recipient explicitly rather than resolving it from
/// <c>ICurrentUser</c> - a Quartz job has no HTTP context to resolve one from. The read
/// side is the opposite: it is only ever reached from an authenticated request, and the
/// handlers pass the caller's own id.
/// </remarks>
public interface IAtsNotificationService
{
	/// <summary>
	/// Persists a notification and pushes it to the recipient if they are connected.
	/// Never throws: a failure to notify must not roll back the operation that caused it.
	/// </summary>
	Task RaiseAsync(
		Guid recipientUserId,
		string type,
		string title,
		string body,
		string? linkUrl,
		Guid? entityId,
		CancellationToken cancellationToken);

	/// <summary>
	/// Raises a notification about one order, addressed to whoever requested it.
	/// </summary>
	/// <remarks>
	/// Resolves the requestor and the subject name from the order itself, so the four
	/// order-shaped events (form submitted, order completed, report ready, ticketing
	/// failed) do not each repeat that lookup or hand-build the same deep link. A no-op
	/// when the order has no requestor - public-API orders have no ATS user to tell.
	/// </remarks>
	Task RaiseForOrderAsync(
		Guid emailInvitationId,
		string type,
		CancellationToken cancellationToken);

	/// <summary>
	/// Tells each uploader when every invitation email for their bulk file has been sent.
	/// </summary>
	/// <remarks>
	/// Given the orders a send pass just finished, works out which of their files are now
	/// complete and raises one notification per file. The email job sends in claimed slices,
	/// so this is called after every pass but only fires on the pass that finishes a file.
	/// </remarks>
	Task RaiseForCompletedBulkEmailsAsync(
		IReadOnlyCollection<Guid> sentEmailInvitationIds,
		CancellationToken cancellationToken);

	Task<KeysetPaginatedResult<NotificationListDTO>> GetNotificationsAsync(
		Guid recipientUserId,
		KeysetPaginationRequest request,
		bool unreadOnly,
		CancellationToken cancellationToken);

	Task<NotificationUnreadCountDTO> GetUnreadCountAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken);

	Task<bool> MarkAsReadAsync(
		Guid recipientUserId,
		Guid notificationId,
		CancellationToken cancellationToken);

	Task<int> MarkAllAsReadAsync(
		Guid recipientUserId,
		CancellationToken cancellationToken);
}
