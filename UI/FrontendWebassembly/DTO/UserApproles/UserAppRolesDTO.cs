namespace FrontendWebassembly.DTO.UserApproles;
public record UserAppRoleDto
{
	public int AppRoleId { get; set; }
	public string? UserEmail { get; set; }
	public string? RoleName { get; set; }
	public string? AppName { get; set; }
	public string? SubMenuName { get; set; }
}

public class EditUserAppRoleDto
{
	public int AppRoleId { get; set; }
	public int UserId { get; set; }
	public int AppId { get; set; }
	public int SubMenuId { get; set; }
	public int RoleId { get; set; }
}
