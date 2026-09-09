namespace ATS.Configuration;

/// <summary>
/// In-app notification tuning, mirroring AtsAuditOptions. Every value has a working default
/// so the feature ships without an appsettings change; bind the "AtsNotifications" section
/// only to override one.
/// </summary>
public sealed class AtsNotificationOptions
{
	public const string SectionName = "AtsNotifications";

	public bool RetentionEnabled { get; set; } = true;

	// One month. A notification older than this has either been acted on or overtaken by
	// the state it was pointing at, and the row is only useful for the badge and the
	// dropdown - neither of which looks back that far.
	public int RetentionDays { get; set; } = 30;

	public int RetentionIntervalHours { get; set; } = 24;

	public int RetentionBatchSize { get; set; } = 5_000;
}
