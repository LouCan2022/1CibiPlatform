namespace Auth.Services;

public class AppSubRoleService : IAppSubRoleService
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<AppSubRoleService> _logger;

	public AppSubRoleService(IAuthRepository authRepository,
					   ILogger<AppSubRoleService> logger)
	{
		_authRepository = authRepository;
		_logger = logger;
	}

	public Task<PaginatedResult<AppSubRolesDTO>> GetAppSubRolesAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var logContext = new
		{
			Action = "GetAppSubRoles",
			Step = "FetchingAppSubRoles",
			Pagination = paginationRequest,
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Fetching appsubrole with pagination: {@Context}", logContext);

		return string.IsNullOrEmpty(paginationRequest.SearchTerm) ?
			_authRepository.GetAppSubRolesAsync(paginationRequest, cancellationToken) :
			_authRepository.SearchAppSubRoleAsync(paginationRequest, cancellationToken);
	}

	public async Task<bool> DeleteAppSubRoleAsync(int AppSubRoleId)
	{
		var logContext = new
		{
			Action = "DeleteAppSubRole",
			Step = "FetchForDelete",
			AppSubRoleId,
			Timestamp = DateTime.UtcNow
		};

		var appSubRole = await _authRepository.GetAppSubRoleAsync(AppSubRoleId);
		if (appSubRole == null)
		{
			_logger.LogError("{AppSubRoleId} was not found during delete operation: {@Context}", AppSubRoleId, logContext);
			throw new NotFoundException($"AppSubRole with ID {AppSubRoleId} was not found.");
		}
		var isDeleted = await _authRepository.DeleteAppSubRoleAsync(appSubRole);
		return isDeleted;
	}

	public async Task<AppSubRoleDTO> EditAppSubRoleAsync(EditAppSubRoleDTO appSubRoleDTO)
	{
		var logContext = new
		{
			Action = "EditAppSubRole",
			Step = "FetchForUpdate",
			AppSubRoleId = appSubRoleDTO.AppSubRoleId,
			Timestamp = DateTime.UtcNow
		};

		var existingAppSubRole = await _authRepository.GetAppSubRoleAsync(appSubRoleDTO.AppSubRoleId);
		if (existingAppSubRole == null)
		{
			_logger.LogError("{AppSubRoleId} was not found during update operation: {@Context}", appSubRoleDTO.AppSubRoleId, logContext);
			throw new NotFoundException($"AppSubRole with ID {appSubRoleDTO.AppSubRoleId} was not found.");
		}

		existingAppSubRole.UserId = appSubRoleDTO.UserId;
		existingAppSubRole.AppId = appSubRoleDTO.AppId!;
		existingAppSubRole.Submenu = appSubRoleDTO.SubMenuId;
		existingAppSubRole.RoleId = appSubRoleDTO.RoleId;

		var application = await _authRepository.EditAppSubRoleAsync(existingAppSubRole);
		return application.Adapt<AppSubRoleDTO>();
	}

	public async Task<bool> AddAppSubRoleAsync(AddAppSubRoleDTO appSubRole)
	{
		var isAdded = await _authRepository.AddAppSubRoleAsync(appSubRole);
		return isAdded;
	}
}
