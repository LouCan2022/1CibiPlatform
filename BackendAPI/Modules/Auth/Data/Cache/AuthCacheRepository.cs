namespace Auth.Data.Cache;

public class AuthCacheRepository : IAuthRepository
{
	private readonly IAuthRepository _authRepository;
	private readonly HybridCache _hybridCache;

	public AuthCacheRepository(
		IAuthRepository authRepository,
		HybridCache hybridCache)
	{
		this._authRepository = authRepository;
		this._hybridCache = hybridCache;
	}

	public async Task<PaginatedResult<UsersDTO>> GetUserAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		var cacheKey = $"users_page_{paginationRequest.PageIndex}_size_{paginationRequest.PageSize}";

		return await _hybridCache.GetOrCreateAsync<PaginationRequest, PaginatedResult<UsersDTO>>(
			cacheKey,
			paginationRequest,
			async (req, token) => await _authRepository.GetUserAsync(req, token),
			null,
			null,
			cancellationToken);
	}

	public async Task<PaginatedResult<UsersDTO>> SearchUserAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var cacheKey = $"users_page_{paginationRequest.PageIndex}_size_{paginationRequest.PageSize}_search_{paginationRequest.SearchTerm}";

		return await _hybridCache.GetOrCreateAsync<PaginationRequest, PaginatedResult<UsersDTO>>(
			cacheKey,
			paginationRequest,
			async (req, token) => await _authRepository.SearchUserAsync(req, token),
			null,
			null,
			cancellationToken);
	}
	public async Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var cacheKey = $"applications_page_{paginationRequest.PageIndex}_size_{paginationRequest.PageSize}";

		return await _hybridCache.GetOrCreateAsync<PaginationRequest, PaginatedResult<ApplicationsDTO>>(
			cacheKey,
			paginationRequest,
			async (req, token) => await _authRepository.GetApplicationsAsync(req, token),
			null,
			null,
			cancellationToken);
	}

	public async Task<PaginatedResult<ApplicationsDTO>> SearchApplicationsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var cacheKey = $"applications_page_{paginationRequest.PageIndex}_size_{paginationRequest.PageSize}_search_{paginationRequest.SearchTerm}";

		return await _hybridCache.GetOrCreateAsync<PaginationRequest, PaginatedResult<ApplicationsDTO>>(
			cacheKey,
			paginationRequest,
			async (req, token) => await _authRepository.SearchApplicationsAsync(req, token),
			null,
			null,
			cancellationToken);
	}

	public async Task<bool> DeleteOtpRecordIfExpired(OtpVerification otpVerification)
	{
		return await _authRepository.DeleteOtpRecordIfExpired(otpVerification);
	}

	public async Task<UserDataDTO> GetNewUserDataAsync(Guid userId)
	{
		return await _authRepository.GetNewUserDataAsync(userId);
	}

	public async Task<Authusers> GetRawUserAsync(Guid id)
	{
		return await _authRepository.GetRawUserAsync(id);
	}

	public async Task<LoginDTO> GetUserDataAsync(LoginWebCred cred)
	{
		return await _authRepository.GetUserDataAsync(cred);
	}

	public async Task<PasswordResetToken> GetUserTokenAsync(string token)
	{
		return await _authRepository.GetUserTokenAsync(token);
	}

	public async Task<bool> InsertOtpVerification(OtpVerification otpVerification)
	{
		return await _authRepository.InsertOtpVerification(otpVerification);
	}

	public async Task<Authusers> IsUserEmailExistAsync(string email)
	{
		return await _authRepository.IsUserEmailExistAsync(email);
	}

	public async Task<OtpVerification> IsUserEmailExistInOtpVerificationAsync(string email, bool isUsed)
	{
		return await _authRepository.IsUserEmailExistInOtpVerificationAsync(email, isUsed);
	}

	public async Task<AuthRefreshToken> IsUserExistAsync(Guid userId)
	{
		return await _authRepository.IsUserExistAsync(userId);
	}

	public async Task<OtpVerification> OtpVerificationUserData(OtpVerificationRequestDTO otpVerificationRequestDTO)
	{
		return await _authRepository.OtpVerificationUserData(otpVerificationRequestDTO);
	}

	public async Task<RegisterResponseDTO> RegisterUserAsync(RegisterRequestDTO userDto)
	{
		return await _authRepository.RegisterUserAsync(userDto);
	}

	public async Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate)
	{
		return await _authRepository.SaveRefreshTokenAsync(userId, hashToken, expiryDate);
	}

	public async Task<bool> SaveToResetPasswordToken(PasswordResetToken passwordResetToken)
	{
		return await _authRepository.SaveToResetPasswordToken(passwordResetToken);
	}

	public async Task<bool> SaveUserAsync(Authusers user)
	{
		return await _authRepository.SaveUserAsync(user);
	}

	public async Task<bool> UpdateAuthUserPassword(Authusers authusers)
	{
		return await _authRepository.UpdateAuthUserPassword(authusers);
	}

	public async Task<bool> UpdatePasswordResetTokenAsUsedAsync(PasswordResetToken passwordResetToken)
	{
		return await _authRepository.UpdatePasswordResetTokenAsUsedAsync(passwordResetToken);
	}

	public async Task<bool> UpdateRevokeReasonAsync(AuthRefreshToken authRefreshToken, string reason)
	{
		return await _authRepository.UpdateRevokeReasonAsync(authRefreshToken, reason);
	}

	public async Task<bool> UpdateValidateOtp(OtpVerification otpVerification)
	{
		return await _authRepository.UpdateValidateOtp(otpVerification);
	}

	public async Task<bool> UpdateVerificationCodeAsync(OtpVerification userDto)
	{
		return await _authRepository.UpdateVerificationCodeAsync(userDto);
	}

	public Task<ApplicationsDTO?> GetApplicationAsync(int applicationId)
	{
		throw new NotImplementedException();
	}

	public Task<bool> DeleteApplicationAsync(int applicationId)
	{
		throw new NotImplementedException();
	}

	public Task<bool> AddApplicationAsync(ApplicationsDTO application)
	{
		throw new NotImplementedException();
	}
}
