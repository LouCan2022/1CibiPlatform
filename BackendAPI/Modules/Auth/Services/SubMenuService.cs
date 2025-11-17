namespace Auth.Services;

public class SubMenuService : ISubMenuService
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<SubMenuService> _logger;

	public SubMenuService(IAuthRepository authRepository,
					   ILogger<SubMenuService> logger)
	{
		_authRepository = authRepository;
		_logger = logger;
	}

	public async Task<bool> AddSubMenuAsync(AddSubMenuDTO subMenu)
	{
		var isAdded = await _authRepository.AddSubMenuAsync(subMenu);
		return isAdded;
	}

	public async Task<bool> DeleteSubMenuAsync(int SubMenuId)
	{
		var subMenu = await _authRepository.GetSubMenuAsync(SubMenuId);
		if (subMenu == null)
		{
			_logger.LogError("SubMenu with ID {SubMenuId} was not found during delete operation.", SubMenuId);
			throw new NotFoundException($"SubMenu with ID {SubMenuId} was not found.");
		}

		var isDeleted = await _authRepository.DeleteSubMenuAsync(subMenu);

		return isDeleted;
	}

	public async Task<SubMenuDTO> EditSubMenuAsync(EditSubMenuDTO subMenuDTO)
	{
		var existingSubMenu = await _authRepository.GetSubMenuAsync(subMenuDTO.SubMenuId);
		if (existingSubMenu == null)
		{
			_logger.LogError("SubMenu with ID {SubMenuId} was not found during update operation.", subMenuDTO!.SubMenuId);
			throw new NotFoundException($"SubMenu with ID {subMenuDTO.SubMenuId} was not found.");
		}
		var subMenu = await _authRepository.EditSubMenuAsync(subMenuDTO);
		return subMenu.Adapt<SubMenuDTO>();
	}

	public Task<PaginatedResult<SubMenusDTO>> GetSubMenusAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Fetching submenus with pagination: {@PaginationRequest}", paginationRequest);

		return string.IsNullOrEmpty(paginationRequest.SearchTerm) ?
			_authRepository.GetSubMenusAsync(paginationRequest, cancellationToken) :
			_authRepository.SearchSubMenusAsync(paginationRequest, cancellationToken);
	}
}
