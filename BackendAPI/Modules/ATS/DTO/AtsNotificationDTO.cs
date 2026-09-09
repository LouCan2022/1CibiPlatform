namespace ATS.Data.DTO;

/// <summary>
/// One notification as the bell and its dropdown render it. Also the SignalR push payload,
/// so a live arrival and a row fetched from the inbox are the same shape and the UI has a
/// single code path for both.
/// </summary>
/// <remarks>
/// CreatedAt and NotificationId are the keyset sort keys, so both survive the projection.
/// RecipientUserId deliberately does not: the caller can only ever read their own
/// notifications, so echoing the id back adds nothing and invites it being trusted.
/// </remarks>
public record NotificationListDTO
{
	public Guid NotificationId { get; set; }

	public DateTime CreatedAt { get; set; }

	public string Type { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;

	public string Body { get; set; } = string.Empty;

	public string? LinkUrl { get; set; }

	public Guid? EntityId { get; set; }

	public bool IsRead { get; set; }
}

/// <summary>
/// The badge value. A record rather than a bare long so the endpoint has a stable response
/// shape to add to later without breaking clients.
/// </summary>
public record NotificationUnreadCountDTO
{
	public long UnreadCount { get; set; }
}

/// <summary>
/// Just enough of an order to address and word a notification about it: who to tell and
/// whose application it was. Internal to the notification senders - never returned by an
/// endpoint.
/// </summary>
public record NotificationOrderTargetDTO
{
	public Guid EmailInvitationId { get; set; }

	// Null for orders placed through the public API, which have no ATS user behind them.
	// A notification with no recipient is simply not raised.
	public Guid? RequestorId { get; set; }

	public string? FirstName { get; set; }

	public string? LastName { get; set; }

	/// <summary>The subject's display name, for the notification body and the deep link.</summary>
	public string SubjectName =>
		string.Join(' ', new[] { FirstName, LastName }
			.Where(part => !string.IsNullOrWhiteSpace(part)));
}
