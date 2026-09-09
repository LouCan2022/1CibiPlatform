namespace ATS.Configuration;

/// <summary>
/// Audit trail tuning, mirroring PlatformLoggingOptions. Every value has a working default
/// so the feature ships without an appsettings change; bind the "AtsAudit" section only to
/// override one.
/// </summary>
public sealed class AtsAuditOptions
{
	public const string SectionName = "AtsAudit";

	public bool Enabled { get; set; } = true;

	// The in-memory queue depth. Full means the oldest write is dropped rather than the
	// request being held up - see AtsAuditWriter.
	public int BufferSize { get; set; } = 10_000;

	// How many entries the drain writes per SaveChanges.
	public int BatchSize { get; set; } = 100;

	public bool RetentionEnabled { get; set; } = true;

	public int RetentionDays { get; set; } = 30;

	public int RetentionIntervalHours { get; set; } = 24;

	public int RetentionBatchSize { get; set; } = 5_000;
}
