namespace FrontendWebassembly.DTO.Auth;

public record AppSubRolesDTO
{
	public int UserSubRoleId { get; set; }
	public Guid UserId { get; set; }
	public int AppId { get; set; }
	public int Submenu { get; set; }
	public int RoleId { get; set; }
}

public record AppSubRolesResponseDTO
{
	public PaginatedResult<AppSubRolesDTO>? appsubroles { get; set; }
}