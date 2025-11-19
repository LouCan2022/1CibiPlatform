namespace Auth.DTO;

public record AppSubRoleDTO
{
	public int AppRoleId { get; set; }
	public Guid UserId { get; set; }
	public int AppId { get; set; }
	public int Submenu { get; set; }
	public int RoleId { get; set; }
}
