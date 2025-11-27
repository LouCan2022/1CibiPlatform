namespace Auth.DTO;

public record AppSubRolesDTO(
	int AppRoleId,
	Guid UserId,
	string? UserEmail,
	int AppId,
	string? AppName,
	int SubMenuId,
	string SubMenuName,
	int RoleId,
	string? RoleName
);
