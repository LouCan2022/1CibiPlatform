namespace Auth.Data.Entities;

public class AuthAttempts
{
	public Guid userId { get; set; }

	public int attempts { get; set; }

	public string message { get; set; }

	public DateTime createAt { get; set; } = DateTime.UtcNow;
}
