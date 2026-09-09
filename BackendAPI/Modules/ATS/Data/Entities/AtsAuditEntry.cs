namespace ATS.Data.Entities;

/// <summary>
/// One state-changing ATS operation, recorded for accountability rather than debugging.
/// This is deliberately not a foreign-keyed row: the trail records who acted at that
/// moment, so it has to survive the user being deactivated, reassigned to another client,
/// or removed outright. That is also why the caller's role, client and super-admin status
/// are copied onto the entry instead of being joined at read time - a role change must not
/// rewrite history.
/// </summary>
public sealed class AtsAuditEntry
{
	public Guid AuditEntryId { get; set; }

	public DateTime OccurredAt { get; set; }

	// The command type name with its suffix trimmed, e.g. "AddUser".
	public string Action { get; set; } = string.Empty;

	// The feature folder the command came from, e.g. "UserManagement".
	public string Area { get; set; } = string.Empty;

	public string Outcome { get; set; } = string.Empty;

	// Null on success. Truncated to the column width: this is diagnostic text, and an
	// over-long exception message must not fail the write that records it.
	public string? FailureReason { get; set; }

	public int DurationMs { get; set; }

	public Guid? UserId { get; set; }

	public string? UserEmail { get; set; }

	public string? UserFullName { get; set; }

	public int? AtsRoleId { get; set; }

	public int? AtsClientId { get; set; }

	// The user's ATS site. Unlike the role and client it is not a claim, so it is resolved
	// from UserDetails by the drain rather than read in the request - see
	// AtsAuditDrainService.ResolveSitesAsync.
	public string? Site { get; set; }

	public bool IsPlatformSuperAdmin { get; set; }

	public string? IpAddress { get; set; }

	// Ties an entry back to the matching Serilog rows in logging.log_events.
	public string? TraceId { get; set; }

	// The command serialized as JSON with sensitive values masked. See AtsAuditRedactor.
	public string Payload { get; set; } = "{}";

	// Field-level before/after values, as EF's change tracker saw them, serialized as a
	// JSON array of AtsEntityChangeDTO. Null when the command changed nothing EF tracked -
	// a create with no tracked save, or one of the ExecuteUpdateAsync paths, which issue
	// SQL directly and never populate the tracker.
	//
	// Unlike Payload these are NOT redacted: the point of a diff is the actual old value.
	// That makes this column as sensitive as the source data, which is why the whole
	// screen is platform-super-admin only.
	public string? Changes { get; set; }
}
