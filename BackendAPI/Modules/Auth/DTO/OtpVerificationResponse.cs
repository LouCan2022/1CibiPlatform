namespace Auth.DTO;


public record OtpVerificationResponse(
	long Id,
	string Email,
	string OtpCodeHash,
	bool IsVerified,
	bool IsUsed,
	int AttemptCount,
	DateTime CreatedAt,
	DateTime ExpiresAt,
	DateTime? VerifiedAt);