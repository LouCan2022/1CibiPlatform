namespace ATS.Services.AuditTrail;

/// <summary>
/// Carries the field-level changes of one request from the DbContext that observed them to
/// the audit behaviour that records them.
/// Scoped, because a request is the unit both ends agree on: the interceptor writes to it
/// inside SaveChanges, and the behaviour reads it once the handler returns. It cannot be
/// done in the drain - by then the DbContext is disposed and the original values are gone.
/// </summary>
public interface IAtsAuditChangeCollector
{
	IReadOnlyList<AtsEntityChangeDTO> Changes { get; }

	/// <summary>
	/// Records what one SaveChanges call modified. Called once per SaveChanges, so a
	/// command that saves twice accumulates both.
	/// </summary>
	void Add(IEnumerable<AtsEntityChangeDTO> changes);
}
