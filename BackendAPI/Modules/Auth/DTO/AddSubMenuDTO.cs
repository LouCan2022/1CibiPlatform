namespace Auth.DTO;

public record AddSubMenuDTO
{
	public string? SubMenuName { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
}
