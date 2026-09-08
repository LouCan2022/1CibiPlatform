namespace ATS.Services.AuditTrail;

public sealed class AtsAuditChangeCollector : IAtsAuditChangeCollector
{
	// Capped so one command cannot accumulate an unbounded list. A bulk upload saves
	// hundreds of rows; past this point the entries stop being read by a human anyway,
	// and the payload already records what the command asked for.
	private const int MaxTrackedEntities = 50;

	private readonly List<AtsEntityChangeDTO> _changes = [];

	public IReadOnlyList<AtsEntityChangeDTO> Changes => _changes;

	public void Add(IEnumerable<AtsEntityChangeDTO> changes)
	{
		foreach (var change in changes)
		{
			if (_changes.Count >= MaxTrackedEntities)
			{
				return;
			}

			_changes.Add(change);
		}
	}
}
