namespace Auth.DTO;

public record AccountNotificationDTO
{
	public string? Gmail { get; set; }
	public string? Application { get; set; }
	public string? SubMenu { get; set; }
	public string?Role { get; set; }
}
