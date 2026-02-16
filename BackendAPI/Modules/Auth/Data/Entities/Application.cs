namespace Auth.Data.Entities;

public class AuthApplication
{
	public int AppId { get; set; }
	public string AppName { get; set; } = null!;
	public string? Description { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


