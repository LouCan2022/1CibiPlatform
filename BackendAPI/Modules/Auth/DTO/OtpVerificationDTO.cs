namespace Auth.DTO;


public record OtpVerificationDTO(
	string Email,
	string FirstName,
	string MiddleName,
	string LastName,
	string PasswordHash,
	string OtpCodeHash,
	bool IsVerified,
	bool IsUsed,
	int AttemptCount,
	DateTime CreatedAt,
	DateTime ExpiresAt,
	DateTime? VerifiedAt)
{
	public string Email { get; set; } = Email;
	public string FirstName { get; set; } = FirstName;
	public string MiddleName { get; set; } = MiddleName;
	public string LastName { get; set; } = LastName;
	public string PasswordHash { get; set; } = PasswordHash;
	public string OtpCodeHash { get; set; } = OtpCodeHash;
	public bool IsVerified { get; set; } = IsVerified;
	public bool IsUsed { get; set; } = IsUsed;
	public int AttemptCount { get; set; } = AttemptCount;
	public DateTime CreatedAt { get; set; } = CreatedAt;
	public DateTime ExpiresAt { get; set; } = ExpiresAt;
	public DateTime? VerifiedAt { get; set; } = VerifiedAt;
}