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

	public async Task<UserDTO> EditUserAsync(EditUserDTO userDTO)
	{
		var logContext = new
		{
			Action = "EditUser",
			Step = "FetchForUpdate",
			userDTO.Email,
			Timestamp = DateTime.UtcNow
		};

		var existingUser = await _authRepository.GetUserAsync(userDTO.Email!);
		if (existingUser == null)
		{
			_logger.LogError("{Email} was not found during update operation: {@Context}", userDTO.Email, logContext);
			throw new NotFoundException($"{userDTO.Email} was not found.");
		}

		existingUser.IsApproved = userDTO.IsApproved;

		var user = await _authRepository.EditUserAsync(existingUser);
		return user.Adapt<UserDTO>();
	}

	public Task<PaginatedResult<UsersDTO>> GetUsersAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		var logContext = new
		{
			Action = "GetUsers",
			Step = "StartFetching",
			PaginationRequest = paginationRequest,
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Fetching users with pagination: {@Context}", logContext);

		return string.IsNullOrEmpty(paginationRequest.SearchTerm) ?
			_authRepository.GetUserAsync(paginationRequest, cancellationToken) :
			_authRepository.SearchUserAsync(paginationRequest, cancellationToken);
	}
}
