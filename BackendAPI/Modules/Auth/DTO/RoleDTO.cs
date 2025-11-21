namespace Auth.DTO;

public record RoleDTO
{
	public int RoleId { get; set; }
	public string? RoleName { get; set; }
	public string? Description { get; set; }
}
