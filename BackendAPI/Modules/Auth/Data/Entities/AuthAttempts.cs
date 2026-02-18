namespace Auth.Data.Entities;

public class AuthAttempts
{
	public Guid UserId { get; set; }
	public string? Email { get; set; }

	public int Attempts { get; set; }

	public string? Message { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public DateTime LockReleaseAt { get; set; }

}
