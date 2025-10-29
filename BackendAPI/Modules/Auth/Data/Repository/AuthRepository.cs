namespace Auth.Data.Repository;

public class AuthRepository : IAuthRepository
{
	private readonly AuthApplicationDbContext _dbcontext;

	public AuthRepository(AuthApplicationDbContext dbcontext)
	{
		this._dbcontext = dbcontext;
	}

	public async Task<Authusers> GetRawUserAsync(Guid id)
	{
		return await _dbcontext.AuthUsers
					 .Where(au => au.Id == id && au.IsActive)
					 .AsNoTracking()
					 .FirstOrDefaultAsync();
	}

	public async Task<LoginDTO> GetUserDataAsync(LoginWebCred cred)
	{
		var userData = await (from user in _dbcontext.AuthUsers
							  join userRole in _dbcontext.AuthUserAppRoles
														 on user.Id equals userRole.UserId into userRolesGroup
							  where user.Email == cred.Username && user.IsActive == true
							  select new LoginDTO(
							   user.Id,
							   user.PasswordHash,
							   user.Email!,
							   user.FirstName!,
							   user.LastName!,
							   user.MiddleName,
							   userRolesGroup.Select(r => r.AppId).Distinct().ToList(),
							   userRolesGroup.GroupBy(r => r.AppId)
											 .Select(g => g.Select(r => r.Submenu).ToList())
											 .ToList(),
							   userRolesGroup.Select(r => r.RoleId).Distinct().ToList()
							  )
			).AsNoTracking()
			 .FirstOrDefaultAsync();


		return userData!;
	}


	public async Task<UserDataDTO> GetNewUserDataAsync(Guid userId)
	{
		var userData = await (from user in _dbcontext.AuthUsers
							  join authRefreshToken in _dbcontext.AuthRefreshToken
														 on user.Id equals authRefreshToken.UserId
							  join userRole in _dbcontext.AuthUserAppRoles
														 on user.Id equals userRole.UserId into userRolesGroup
							  where user.Id == authRefreshToken.UserId &&
							  user.IsActive == true && authRefreshToken.IsActive == true
							  select new UserDataDTO(
							   user.Id,
							   user.PasswordHash,
							   user.Email!,
							   user.FirstName!,
							   user.LastName!,
							   user.MiddleName,
							   authRefreshToken.TokenHash,
							   userRolesGroup.Select(r => r.AppId).Distinct().ToList(),
							   userRolesGroup.GroupBy(r => r.AppId)
											 .Select(g => g.Select(r => r.Submenu).ToList())
											 .ToList(),
							   userRolesGroup.Select(r => r.RoleId).Distinct().ToList()
							  )
			).AsNoTracking()
			 .FirstOrDefaultAsync();


		return userData!;
	}

	public async Task<PasswordResetToken> GetUserTokenAsync(string tokenHash)
	{
		var passwordResetToken = await _dbcontext.PasswordResetToken
			.Where(prt => prt.TokenHash == tokenHash &&
						  prt.IsUsed == false)
			.AsNoTracking()
			.FirstOrDefaultAsync();

		return passwordResetToken!;
	}


	public async Task<bool> SaveUserAsync(Authusers user)
	{
		await _dbcontext.AuthUsers.AddAsync(user);

		var result = await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<bool> SaveRefreshTokenAsync(
		Guid userId,
		string hashToken,
		DateTime expiryDate)
	{
		await _dbcontext.AuthRefreshToken.AddAsync(new AuthRefreshToken
		{
			UserId = userId,
			TokenHash = hashToken,
			ExpiresAt = expiryDate
		});

		await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<bool> UpdateRevokeReasonAsync(
		AuthRefreshToken authRefresh,
		string reason)
	{

		authRefresh!.RevokedReason = reason;
		authRefresh.IsActive = false;
		authRefresh.RevokedAt = DateTime.UtcNow;

		_dbcontext.AuthRefreshToken.Update(authRefresh);

		await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<AuthRefreshToken> IsUserExistAsync(Guid userId)
	{
		return await
			_dbcontext.AuthRefreshToken
			.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.IsActive);
	}


	public async Task<Authusers> IsUserEmailExistAsync(string email)
	{
		return await
			_dbcontext.AuthUsers
			.FirstOrDefaultAsync(au => au.Email == email && au.IsActive);
	}

	public async Task<RegisterResponseDTO> RegisterUserAsync(RegisterRequestDTO userDto)
	{
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = userDto.Email,
			PasswordHash = userDto.PasswordHash,
			FirstName = userDto.FirstName,
			LastName = userDto.LastName,
			MiddleName = userDto.MiddleName,
		};

		await _dbcontext.AddAsync(user);

		await _dbcontext.SaveChangesAsync();

		return new RegisterResponseDTO(
			user.Id,
			user.Email!,
			user.PasswordHash!,
			user.FirstName!,
			user.LastName!,
			user.MiddleName);
	}

	public async Task<bool> UpdateVerificationCodeAsync(OtpVerification otpVerification)
	{

		_dbcontext.OtpVerification.Update(otpVerification);

		await _dbcontext.SaveChangesAsync();

		return true;

	}

	public async Task<bool> InsertOtpVerification(OtpVerification otpVerification)
	{

		var otpUser = await _dbcontext.OtpVerification.AddAsync(otpVerification);

		await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<OtpVerification> IsUserEmailExistInOtpVerificationAsync(string email, bool isUsed)
	{
		return await _dbcontext.OtpVerification
					 .Where(ov => ov.Email == email && ov.IsUsed == isUsed)
					 .FirstOrDefaultAsync();

	}

	public async Task<bool> UpdateValidateOtp(OtpVerification otpVerification)
	{
		_dbcontext.OtpVerification.Update(otpVerification);

		await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<bool> DeleteOtpRecordIfExpired(OtpVerification otpVerification)
	{
		_dbcontext.OtpVerification.Remove(otpVerification);

		await _dbcontext.SaveChangesAsync();

		return true;

	}

	public async Task<OtpVerification> OtpVerificationUserData(OtpVerificationRequestDTO otpVerification)
	{
		return await _dbcontext.OtpVerification
					 .Where(ov => ov.Email == otpVerification.email &&
							ov.OtpId == otpVerification.userId &&
							ov.IsUsed == false &&
							ov.IsVerified == false &&
							ov.ExpiresAt > DateTime.UtcNow)
					 .AsNoTracking()
					 .FirstOrDefaultAsync();
	}

	public async Task<bool> SaveToResetPasswordToken(PasswordResetToken passwordResetToken)
	{

		await _dbcontext.PasswordResetToken.AddAsync(passwordResetToken);

		await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<bool> UpdateAuthUserPassword(Authusers authusers)
	{
		_dbcontext.AuthUsers.Update(authusers);

		await _dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<bool> UpdatePasswordResetTokenAsUsedAsync(PasswordResetToken passwordResetToken)
	{
		_dbcontext.PasswordResetToken.Update(passwordResetToken);

		await _dbcontext.SaveChangesAsync();

		return true;
	}
}
