namespace FrontendWebassembly.DTO.Auth;

public record AssignmentNotificationDTO
{
	public string? Gmail { get; set; }
	public string? Application { get; set; }
	public string? SubMenu { get; set; }
	public string? Role { get; set; }
}

public record AddAppSubRoleResult(
	AddAppSubRoleDTO AppSubRole,
	AssignmentNotificationDTO Notification
);
