namespace Auth.Services;

public class UserManagementService : IUserService
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<UserManagementService> _logger;

	public UserManagementService(IAuthRepository authRepository,
					   ILogger<UserManagementService> logger)
	{
		this._authRepository = authRepository;
		this._logger = logger;
	}

	public Task<PaginatedResult<UsersDTO>> GetUsersAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching users with pagination: {@PaginationRequest}", paginationRequest);

		return string.IsNullOrEmpty(paginationRequest.SearchTerm) ?
			_authRepository.GetUserAsync(paginationRequest, cancellationToken) :
			_authRepository.SearchUserAsync(paginationRequest, cancellationToken);
	}
}
