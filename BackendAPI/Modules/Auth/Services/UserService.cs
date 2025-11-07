namespace Auth.Services;

public class UserService : IUserService
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<UserService> _logger;

	public UserService(IAuthRepository authRepository,
					   ILogger<UserService> logger)
	{
		this._authRepository = authRepository;
		this._logger = logger;
	}

	public Task<PaginatedResult<UsersDTO>> GetUsersAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching users with pagination: {@PaginationRequest}", paginationRequest);

		return _authRepository.GetUserAsync(paginationRequest, cancellationToken);
	}
}
