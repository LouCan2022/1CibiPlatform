namespace Auth.Services;

public class AppSubRoleService : IAppSubRoleService
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<ApplicationService> _logger;

	public AppSubRoleService(IAuthRepository authRepository,
					   ILogger<ApplicationService> logger)
	{
		_authRepository = authRepository;
		_logger = logger;
	}

	public Task<PaginatedResult<AppSubRolesDTO>> GetAppSubRolesAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching appsubrole with pagination: {@PaginationRequest}", paginationRequest);

		return string.IsNullOrEmpty(paginationRequest.SearchTerm) ?
			_authRepository.GetAppSubRolesAsync(paginationRequest, cancellationToken) :
			_authRepository.SearchAppSubRoleAsync(paginationRequest, cancellationToken);
	}

	public async Task<bool> DeleteAppSubRoleAsync(int AppSubRoleId)
	{
		var appSubRole = await _authRepository.GetAppSubRoleAsync(AppSubRoleId);
		if (appSubRole == null)
		{
			_logger.LogError("AppSubRole with ID {AppSubRoleId} was not found during delete operation.", AppSubRoleId);
			throw new NotFoundException($"AppSubRole with ID {AppSubRoleId} was not found.");
		}
		var isDeleted = await _authRepository.DeleteAppSubRoleAsync(appSubRole);
		return isDeleted;
	}

	public async Task<AppSubRoleDTO> EditAppSubRoleAsync(EditAppSubRoleDTO appSubRoleDTO)
	{
		var existingAppSubRole = await _authRepository.GetAppSubRoleAsync(appSubRoleDTO.AppSubRoleId);
		if (existingAppSubRole == null)
		{
			_logger.LogError("AppSubRole with ID {AppSubRoleId} was not found during update operation.", appSubRoleDTO.AppSubRoleId);
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
