namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IUserManagementService
{
	Task<PaginatedResult<UsersDTO>> GetUsersAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken cancellationToken = default);
	Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken cancellationToken = default);
	Task<PaginatedResult<SubMenusDTO>> GetSubMenusAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken cancellationToken = default);
	Task<PaginatedResult<RolesDTO>> GetRolesAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken cancellationToken = default);
	Task<PaginatedResult<AppSubRolesDTO>> GetAppSubRolesAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken cancellationToken = default);

	Task<bool> DeleteApplicationAsync(int AppId);
	Task<bool> DeleteSubMenuAsync(int SubMenuId);
	Task<bool> DeleteRoleAsync(int RoleId);
	Task<bool> DeleteUserAppSbRoleAsync(int AppSubRoleId);

	Task<bool> AddApplicationAsync(AddApplicationDTO addApplicationDTO);
}
