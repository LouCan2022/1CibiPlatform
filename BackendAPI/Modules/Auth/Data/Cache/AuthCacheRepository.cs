using Microsoft.Extensions.Caching.Hybrid;

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

	public Task<bool> DeleteOtpRecordIfExpired(OtpVerification otpVerification)
	{
		throw new NotImplementedException();
	}

	public Task<UserDataDTO> GetNewUserDataAsync(Guid userId)
	{
		throw new NotImplementedException();
	}

	public Task<Authusers> GetRawUserAsync(Guid id)
	{
		throw new NotImplementedException();
	}


	public Task<LoginDTO> GetUserDataAsync(LoginWebCred cred)
	{
		throw new NotImplementedException();
	}

	public Task<PasswordResetToken> GetUserTokenAsync(string token)
	{
		throw new NotImplementedException();
	}

	public Task<bool> InsertOtpVerification(OtpVerification otpVerification)
	{
		throw new NotImplementedException();
	}

	public Task<Authusers> IsUserEmailExistAsync(string email)
	{
		throw new NotImplementedException();
	}

	public Task<OtpVerification> IsUserEmailExistInOtpVerificationAsync(string email, bool isUsed)
	{
		throw new NotImplementedException();
	}

	public Task<AuthRefreshToken> IsUserExistAsync(Guid userId)
	{
		throw new NotImplementedException();
	}

	public Task<OtpVerification> OtpVerificationUserData(OtpVerificationRequestDTO otpVerificationRequestDTO)
	{
		throw new NotImplementedException();
	}

	public Task<RegisterResponseDTO> RegisterUserAsync(RegisterRequestDTO userDto)
	{
		throw new NotImplementedException();
	}

	public Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate)
	{
		throw new NotImplementedException();
	}

	public Task<bool> SaveToResetPasswordToken(PasswordResetToken passwordResetToken)
	{
		throw new NotImplementedException();
	}

	public Task<bool> SaveUserAsync(Authusers user)
	{
		throw new NotImplementedException();
	}

	public Task<bool> UpdateAuthUserPassword(Authusers authusers)
	{
		throw new NotImplementedException();
	}

	public Task<bool> UpdatePasswordResetTokenAsUsedAsync(PasswordResetToken passwordResetToken)
	{
		throw new NotImplementedException();
	}

	public Task<bool> UpdateRevokeReasonAsync(AuthRefreshToken authRefreshToken, string reason)
	{
		throw new NotImplementedException();
	}

	public Task<bool> UpdateValidateOtp(OtpVerification otpVerification)
	{
		throw new NotImplementedException();
	}

	public Task<bool> UpdateVerificationCodeAsync(OtpVerification userDto)
	{
		throw new NotImplementedException();
	}
}
