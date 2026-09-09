namespace ATS.Data.Entities;

/// <summary>
/// One in-app notification addressed to a single ATS user.
/// </summary>
/// <remarks>
/// Persisted rather than pushed-and-forgotten because SignalR only reaches a live
/// connection: a user who is logged out when their bulk upload finishes would otherwise
/// never learn it did. The row is written first and pushed second, so the inbox is the
/// source of truth and the push is only an optimisation for someone who happens to be
/// looking.
///
/// Deliberately not foreign-keyed to UserDetails, for the same reason as AtsAuditEntry:
/// the notification records who was told at that moment and must survive the recipient
/// being reassigned or deactivated. <see cref="EntityId"/> is likewise a loose reference -
/// the order it points at may be purged long before the retention sweep reaches this row.
/// </remarks>
public sealed class AtsNotification
{
	public Guid NotificationId { get; set; }

	// The ATS user this is addressed to. Matches the SignalR group name produced by
	// HubCallerContextExtensions.GetUserGroupName, which is what makes the live push and
	// the stored row agree on the recipient.
	public Guid RecipientUserId { get; set; }

	// AtsNotificationType, stored as its string name rather than an int so a row stays
	// readable in the database and reordering the enum cannot silently retype history.
	public string Type { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;

	public string Body { get; set; } = string.Empty;

	// Where clicking the notification takes the user, as an app-relative path built by the
	// sender (for example "/s&i/ats/searchreport?search=Juan+Dela+Cruz"). Null when the
	// event has no screen worth opening. The UI still checks the recipient's module access
	// before rendering it as a link - see NotificationCenter.
	public string? LinkUrl { get; set; }

	// The order or bulk file the notification is about. Kept separately from LinkUrl so a
	// future screen can group or de-duplicate by subject without parsing a URL.
	public Guid? EntityId { get; set; }

	public bool IsRead { get; set; }

	public DateTime? ReadAt { get; set; }

	public DateTime CreatedAt { get; set; }
}
