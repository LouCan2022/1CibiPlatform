namespace Auth.Data.Entities;

public class AuthSubMenu
{
	public int SubMenuId { get; set; }
	public string SubMenuName { get; set; } = string.Empty;
	public string? Description { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; }
}