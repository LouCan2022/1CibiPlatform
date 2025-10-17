namespace Auth.Data.Repository;

public interface IAuthRepository
{

	Task<LoginDTO> GetUserDataAsync(LoginWebCred cred);

	Task<bool> SaveUserAsync(Authusers user);

	Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate);

	Task<UserDataDTO> GetNewUserDataAsync(Guid userId);

	Task<bool> UpdateRevokeReasonAsync(AuthRefreshToken authRefreshToken, string reason);

	Task<AuthRefreshToken> IsUserExistAsync(Guid userId);

	Task<bool> InsertOtpVerification(OtpVerification otpVerification);

	Task<Authusers> IsUserEmailExistAsync(string email);

	Task<OtpVerification> IsUserEmailExistInOtpVerificationAsync(string email, bool isUsed);

	Task<RegisterResponseDTO> RegisterUserAsync(RegisterRequestDTO userDto);

	Task<bool> UpdateVerificationCodeAsync(OtpVerification userDto);

	Task<bool> UpdateValidateOtp(OtpVerification otpVerification);

	Task<bool> DeleteOtpRecordIfExpired(OtpVerification otpVerification);

	Task<OtpVerification> OtpVerificationUserData(OtpVerificationRequestDTO otpVerificationRequestDTO);

}
