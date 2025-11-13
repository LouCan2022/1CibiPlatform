namespace Auth.Services;

public class ApplicationService : IApplicationService
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<ApplicationService> _logger;

	public ApplicationService(IAuthRepository authRepository,
					   ILogger<ApplicationService> logger)
	{
		this._authRepository = authRepository;
		this._logger = logger;
	}

	public Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching users with pagination: {@PaginationRequest}", paginationRequest);

		return string.IsNullOrEmpty(paginationRequest.SearchTerm) ?
			_authRepository.GetApplicationsAsync(paginationRequest, cancellationToken) :
			_authRepository.SearchApplicationsAsync(paginationRequest, cancellationToken);
	}
}
