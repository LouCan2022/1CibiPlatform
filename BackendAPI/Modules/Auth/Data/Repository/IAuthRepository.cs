namespace Auth.Data.Repository;

public interface IAuthRepository
{
	Task<PaginatedResult<UsersDTO>> GetUserAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);

	Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PaginatedResult<SubMenusDTO>> GetSubMenusAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PaginatedResult<AppSubRolesDTO>> GetAppSubRolesAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PaginatedResult<RolesDTO>> GetRolesAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<LoginDTO> GetUserDataAsync(LoginWebCred cred);

	Task<UserDataDTO> GetNewUserDataAsync(Guid userId);

	Task<Authusers> GetRawUserAsync(Guid id);

	Task<PasswordResetToken> GetUserTokenAsync(string token);

	Task<PaginatedResult<UsersDTO>> SearchUserAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	
	Task<PaginatedResult<ApplicationsDTO>> SearchApplicationsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PaginatedResult<SubMenusDTO>> SearchSubMenusAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PaginatedResult<AppSubRolesDTO>> SearchAppSubRoleAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PaginatedResult<RolesDTO>> SearchRoleAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<bool> SaveUserAsync(Authusers user);

	Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate);

	Task<bool> UpdateRevokeReasonAsync(AuthRefreshToken authRefreshToken, string reason);

	Task<bool> UpdateAuthUserPassword(Authusers authusers);

	Task<bool> UpdatePasswordResetTokenAsUsedAsync(PasswordResetToken passwordResetToken);

	Task<AuthRefreshToken> IsUserExistAsync(Guid userId);

	Task<bool> InsertOtpVerification(OtpVerification otpVerification);

	Task<Authusers> IsUserEmailExistAsync(string email);

	Task<OtpVerification> IsUserEmailExistInOtpVerificationAsync(string email, bool isUsed);

	Task<RegisterResponseDTO> RegisterUserAsync(RegisterRequestDTO userDto);

	Task<bool> UpdateVerificationCodeAsync(OtpVerification userDto);

	Task<bool> UpdateValidateOtp(OtpVerification otpVerification);

	Task<bool> DeleteOtpRecordIfExpired(OtpVerification otpVerification);

	Task<OtpVerification> OtpVerificationUserData(OtpVerificationRequestDTO otpVerificationRequestDTO);

	Task<bool> SaveToResetPasswordToken(PasswordResetToken passwordResetToken);

	Task<AuthApplication> GetApplicationAsync(int applicationId);
	Task<AuthUserAppRole> GetAppSubRoleAsync(int appSubRoleId);

	Task<bool> DeleteApplicationAsync(AuthApplication application);
	Task<bool> AddApplicationAsync(AddApplicationDTO application);
	Task<AuthApplication> EditApplicationAsync(AuthApplication application);
	Task<bool> AddSubMenuAsync(AddSubMenuDTO subMenu);
	Task<bool> DeleteSubMenuAsync(AuthSubMenu subMenu);
	Task<AuthSubMenu> GetSubMenuAsync(int applicationId);
	Task<AuthSubMenu> EditSubMenuAsync(AuthSubMenu subMenu);

	Task<bool> AddAppSubRoleAsync(AddAppSubRoleDTO appSubRole);
	Task<bool> DeleteAppSubRoleAsync(AuthUserAppRole appSubRole);
	Task<AuthUserAppRole> EditAppSubRoleAsync(AuthUserAppRole appSubRole);
	Task<bool> AddRoleAsync(AddRoleDTO role);
	Task<bool> DeleteRoleAsync(AuthRole role);
	Task<AuthRole> GetRoleAsync(int roleId);
	Task<AuthRole> EditRoleAsync(AuthRole role);
}
