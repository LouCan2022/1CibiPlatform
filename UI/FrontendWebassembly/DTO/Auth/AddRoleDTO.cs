namespace FrontendWebassembly.DTO.Auth;

public record AddRoleDTO
{
	public string? RoleName { get; set; }
	public string? Description { get; set; }
}
