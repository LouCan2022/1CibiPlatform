namespace ATS.Data.DTO;

// One row of the audit trail screen. OccurredAt and AuditEntryId are the keyset sort keys,
// so both survive the projection.
public record AuditTrailListDTO
{
	public Guid AuditEntryId { get; set; }

	public DateTime OccurredAt { get; set; }

	public string Action { get; set; } = string.Empty;

	public string Area { get; set; } = string.Empty;

	public string Outcome { get; set; } = string.Empty;

	public string? FailureReason { get; set; }

	public int DurationMs { get; set; }

	public Guid? UserId { get; set; }

	public string? UserEmail { get; set; }

	public string? UserFullName { get; set; }

	public int? AtsRoleId { get; set; }

	public int? AtsClientId { get; set; }

	public string? Site { get; set; }

	public bool IsPlatformSuperAdmin { get; set; }

	public string? IpAddress { get; set; }

	public string? TraceId { get; set; }

	// The redacted command JSON, shown in the detail panel rather than the grid.
	public string Payload { get; set; } = "{}";

	// Field-level before/after values as a JSON array. Null when the command changed
	// nothing EF tracked.
	public string? Changes { get; set; }
}

public record AuditOutcomeCountsDTO
{
	public long Success { get; set; }

	public long Failure { get; set; }

	public long Total { get; set; }
}
