namespace ATS.Data.Repository.AuditTrail;

public interface IAtsAuditRepository
{
	Task<List<AuditTrailListDTO>> GetAuditTrailPageAsync(
		DateTime? afterOccurredAt,
		Guid? afterEntryId,
		int take,
		string? outcome,
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken);

	Task<long> CountAuditTrailAsync(
		string? outcome,
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken);

	Task<AuditOutcomeCountsDTO> GetOutcomeCountsAsync(
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken);
}
