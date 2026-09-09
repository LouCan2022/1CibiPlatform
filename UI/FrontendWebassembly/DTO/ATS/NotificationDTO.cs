namespace FrontendWebassembly.DTO.ATS;

/// <summary>
/// One notification, as returned by the inbox endpoints and as pushed over SignalR.
/// </summary>
/// <remarks>
/// Deliberately the same shape on both paths, so the bell has one code path for a live
/// arrival and a row fetched from the server. Mirrors ATS.Data.DTO.NotificationListDTO -
/// keep the property names in step or the JSON stops binding.
/// </remarks>
public class NotificationDTO
{
	public Guid NotificationId { get; set; }

	public DateTime CreatedAt { get; set; }

	public string Type { get; set; } = string.Empty;

	public string Title { get; set; } = string.Empty;

	public string Body { get; set; } = string.Empty;

	// App-relative path built by the server. The UI checks module access before rendering
	// it as a link, so a notification never deep-links somewhere the user cannot open.
	public string? LinkUrl { get; set; }

	public Guid? EntityId { get; set; }

	public bool IsRead { get; set; }
}

public class NotificationUnreadCountDTO
{
	public long UnreadCount { get; set; }
}

public class GetNotificationsResponseDTO
{
	public KeysetPaginatedResult<NotificationDTO>? Notifications { get; set; }
}

public class GetUnreadNotificationCountResponseDTO
{
	public NotificationUnreadCountDTO? Count { get; set; }
}
