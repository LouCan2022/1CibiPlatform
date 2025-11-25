namespace FrontendWebassembly.DTO.Auth;

public record AddAppSubRoleDTO
{
	public Guid UserId { get; set; }
	public int AppId { get; set; }
	public int SubMenuId { get; set; }
	public int RoleId { get; set; }
	public Guid AssignedBy { get; set; }
}
