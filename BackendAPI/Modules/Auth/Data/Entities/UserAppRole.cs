namespace Auth.Data.Entities;

public class AuthUserAppRole
{
	public int AppRoleId { get; set; }
	public Guid UserId { get; set; }
	public int AppId { get; set; }
	public int Submenu { get; set; }
	public int RoleId { get; set; }
	public Guid AssignedBy { get; set; }
	public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}