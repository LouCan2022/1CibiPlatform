namespace Auth.Services;

public interface IApplicationService
{
	Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken);
}
