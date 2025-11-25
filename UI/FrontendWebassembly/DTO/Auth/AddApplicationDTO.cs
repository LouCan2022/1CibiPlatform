namespace FrontendWebassembly.DTO.Auth;

public record AddApplicationDTO
{
	public string? AppName { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
}
