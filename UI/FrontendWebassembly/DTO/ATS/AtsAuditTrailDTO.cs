namespace FrontendWebassembly.DTO.ATS;

public record AuditTrailListDTO
{
	public Guid AuditEntryId { get; set; }

	public DateTime OccurredAt { get; set; }

	// The command name with its suffix trimmed, e.g. "AddUser".
	public string Action { get; set; } = string.Empty;

	// The feature area the action came from, e.g. "UserManagement".
	public string Area { get; set; } = string.Empty;

	public string Outcome { get; set; } = string.Empty;

	// Why the action failed; rendered beneath a Failure row.
	public string? FailureReason { get; set; }

	public int DurationMs { get; set; }

	public Guid? UserId { get; set; }

	public string? UserEmail { get; set; }

	public string? UserFullName { get; set; }

	public int? AtsRoleId { get; set; }

	public int? AtsClientId { get; set; }

	// The user's ATS site at the time of the action.
	public string? Site { get; set; }

	public bool IsPlatformSuperAdmin { get; set; }

	public string? IpAddress { get; set; }

	public string? TraceId { get; set; }

	// The command as JSON with sensitive values masked, shown in the detail panel.
	public string Payload { get; set; } = "{}";

	// Field-level before/after values as a JSON array of AuditEntityChangeDTO. Null when
	// the action changed nothing EF tracked, which the dialog reports rather than hiding.
	public string? Changes { get; set; }
}

// Mirrors ATS.Data.DTO.AtsEntityChangeDTO, deserialized from the Changes JSON for display.
public record AuditEntityChangeDTO
{
	public string Entity { get; set; } = string.Empty;

	public string? Key { get; set; }

	public string State { get; set; } = string.Empty;

	public Dictionary<string, AuditPropertyChangeDTO> Changes { get; set; } = [];
}

public record AuditPropertyChangeDTO
{
	public string? From { get; set; }

	public string? To { get; set; }
}

public record AuditOutcomeCountsDTO
{
	public long Success { get; set; }

	public long Failure { get; set; }

	public long Total { get; set; }
}

// Response envelopes, matching the property names the Carter endpoints return.
public record GetAuditTrailResponseDTO
{
	public KeysetPaginatedResult<AuditTrailListDTO>? AuditEntries { get; set; }
}

public record GetAuditOutcomeCountsResponseDTO
{
	public AuditOutcomeCountsDTO? Counts { get; set; }
}

// Mirrors ATS.Constants.AuditOutcome, which lives in the backend assembly and is not
// referenced by the UI project.
public static class AuditActionOutcome
{
	public const string Success = "Success";

	public const string Failure = "Failure";
}
