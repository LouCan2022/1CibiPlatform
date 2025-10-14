namespace Auth.Data.Entities;

public class PasswordResetToken
{
	public long Id { get; set; }
	public Guid UserId { get; set; }
	public string TokenHash { get; set; } = string.Empty;
	public bool IsUsed { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public DateTime? UsedAt { get; set; }
}