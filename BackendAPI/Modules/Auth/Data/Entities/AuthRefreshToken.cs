namespace Auth.Data.Entities;

public class AuthRefreshToken
{
	public int Id { get; set; }
	public Guid UserId { get; set; }
	public string TokenHash { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public DateTime? RevokedAt { get; set; }
	public string? RevokedReason { get; set; }
}
