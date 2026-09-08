namespace ATS.Shared.Contracts;

public interface IAtsAuditWriter
{
	/// <summary>
	/// Queues one entry for the background drain to persist. Returns false when the entry
	/// was dropped because the queue is full.
	/// Never blocks and never throws: an audit failure must not fail the request that was
	/// being audited.
	/// </summary>
	bool TryEnqueue(AtsAuditEntry entry);
}
