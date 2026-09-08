namespace ATS.Data.DTO;

/// <summary>
/// One property whose value a command changed.
/// </summary>
public sealed record AtsPropertyChangeDTO(string? From, string? To);

/// <summary>
/// The changed properties of one entity, as EF Core's change tracker saw them at
/// SaveChanges. Only modified properties appear - an edit that touched two fields records
/// two, not the whole row.
/// </summary>
public sealed record AtsEntityChangeDTO
{
	// The entity type name, e.g. "PackageDetails".
	public string Entity { get; set; } = string.Empty;

	// The primary key value, so the row can be identified. Composite keys are joined.
	public string? Key { get; set; }

	// Added / Modified / Deleted.
	public string State { get; set; } = string.Empty;

	public Dictionary<string, AtsPropertyChangeDTO> Changes { get; set; } = [];
}
