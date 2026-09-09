namespace ATS.Constants;

/// <summary>
/// The kinds of in-app notification ATS raises. Stored as the string name on
/// <see cref="ATS.Data.Entities.AtsNotification.Type"/>.
/// </summary>
/// <remarks>
/// Strings rather than an enum for the same reason as OrderStatus and OrderHistoryEventType:
/// the value is persisted, so it has to stay readable in the database and survive someone
/// reordering the list. The UI maps each of these to an icon and an accent colour - adding
/// a value here without adding that mapping falls back to the neutral treatment rather
/// than breaking.
/// </remarks>
public static class AtsNotificationType
{
	/// <summary>A candidate completed and submitted the application form we sent them.</summary>
	public const string ApplicationFormSubmitted = "ApplicationFormSubmitted";

	/// <summary>A bulk upload finished parsing; the body carries the accepted/rejected counts.</summary>
	public const string BulkUploadCompleted = "BulkUploadCompleted";

	/// <summary>An order reached its terminal Completed state.</summary>
	public const string OrderCompleted = "OrderCompleted";

	/// <summary>A report was uploaded against an order and is ready to read.</summary>
	public const string ReportReady = "ReportReady";

	/// <summary>An order was marked disputed and needs someone to look at it.</summary>
	public const string OrderDisputed = "OrderDisputed";

	/// <summary>OMS ticketing exhausted its automatic retries; the order needs a manual retry.</summary>
	public const string TicketingFailed = "TicketingFailed";

	/// <summary>The invitation email could not be delivered after the configured attempts.</summary>
	public const string InvitationEmailFailed = "InvitationEmailFailed";
}
