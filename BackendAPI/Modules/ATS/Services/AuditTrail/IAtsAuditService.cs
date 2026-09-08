namespace ATS.Services.AuditTrail;

public interface IAtsAuditService
{
	Task<KeysetPaginatedResult<AuditTrailListDTO>> GetAuditTrailAsync(
		KeysetPaginationRequest paginationRequest,
		string? outcome,
		string? action,
		string? area,
		CancellationToken cancellationToken);

	Task<AuditOutcomeCountsDTO> GetOutcomeCountsAsync(
		string? action,
		string? area,
		string? searchTerm,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken);
}
