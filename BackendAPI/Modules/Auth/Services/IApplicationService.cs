namespace Auth.Services;
public interface IApplicationService
{
	Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken);

	Task<bool> DeleteApplicationAsync(int AppId);

	Task<bool> AddApplicationAsync(ApplicationsDTO application);
}
