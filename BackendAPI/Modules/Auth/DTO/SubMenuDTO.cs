namespace Auth.DTO;

public record SubMenuDTO
{
	public int SubMenuId { get; set; }
	public string? SubMenuName { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
}
