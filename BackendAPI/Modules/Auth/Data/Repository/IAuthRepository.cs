namespace Auth.Data.Repository;

public interface IAuthRepository
{

	Task<LoginDTO> GetUserDataAsync(LoginWebCred cred);

	Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate);

	Task<UserDataDTO> GetNewUserDataAsync(Guid userId);

	Task<bool> UpdateRevokeReasonAsync(AuthRefreshToken authRefreshToken, string reason);

	Task<AuthRefreshToken> IsUserExistAsync(Guid userId);

	Task<OtpVerification> InsertOtpVerification(OtpVerificationDTO otpVerificationDTO);

	Task<Authusers> IsUserEmailExistAsync(string email);

	Task<RegisterResponseDTO> RegisterUserAsync(RegisterRequestDTO userDto);

	Task<bool> UpdateVerificationCodeAsync(OtpVerification userDto);
}
