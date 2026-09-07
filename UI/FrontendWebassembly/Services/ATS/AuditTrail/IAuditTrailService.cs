namespace FrontendWebassembly.Services.ATS.AuditTrail;

public interface IAuditTrailService
{
	// A caller who is not a platform super admin reads an empty page rather than an error,
	// which is how every other ATS list behaves.
	Task<ServiceResponse<KeysetPaginatedResult<AuditTrailListDTO>>> GetAuditTrailAsync(
		string? cursor = null,
		int? pageSize = 10,
		string? outcome = null,
		string? action = null,
		string? area = null,
		string? searchTerm = null,
		DateTime? startDate = null,
		DateTime? endDate = null);

	Task<ServiceResponse<AuditOutcomeCountsDTO>> GetOutcomeCountsAsync(
		string? action = null,
		string? area = null,
		string? searchTerm = null,
		DateTime? startDate = null,
		DateTime? endDate = null);
}
