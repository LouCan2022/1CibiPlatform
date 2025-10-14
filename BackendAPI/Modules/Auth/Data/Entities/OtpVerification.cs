namespace Auth.Data.Entities;

public class OtpVerification
{
	public long Id { get; set; }

	public string Email { get; set; } = string.Empty;

	public string OtpCodeHash { get; set; } = string.Empty;

	public bool IsVerified { get; set; }

	public bool IsUsed { get; set; }

	public int AttemptCount { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime ExpiresAt { get; set; }

	public DateTime? VerifiedAt { get; set; }

}
