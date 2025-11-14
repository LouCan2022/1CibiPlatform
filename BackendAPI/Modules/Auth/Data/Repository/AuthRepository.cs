namespace Auth.Data.Repository;

public class AuthRepository : IAuthRepository
{
	private readonly AuthApplicationDbContext _dbcontext;

	public AuthRepository(AuthApplicationDbContext dbcontext)
	{
		this._dbcontext = dbcontext;
	}

	public async Task<PaginatedResult<UsersDTO>> GetUserAsync(
		PaginationRequest paginationRequest,
		CancellationToken cancellationToken)
	{
		var totalRecords = await _dbcontext
			.AuthUsers
			.Where(au => au.IsActive)
			.LongCountAsync(cancellationToken);

		var users = await _dbcontext.AuthUsers
			.Where(au => au.IsActive)
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(au => new UsersDTO(
					au.Id,
					au.Email,
					au.FirstName,
					au.MiddleName!,
					au.LastName))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<UsersDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  users
			);
	}
	public async Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var totalRecords = await _dbcontext
			.AuthSubmenu
			.Where(aa => aa.IsActive)
			.LongCountAsync(cancellationToken);

		var applications = await _dbcontext.AuthApplications
			.Where(aa => aa.IsActive)
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(aa => new ApplicationsDTO(
					aa.AppId,
					aa.AppName,
					aa.Description!))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<ApplicationsDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  applications
			);
	}
	public async Task<PaginatedResult<UsersDTO>> SearchUserAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{

		var usersQuery = _dbcontext.AuthUsers
				.Where(au => au.IsActive &&
					(EF.Functions.ILike(au.FirstName, $"%{paginationRequest.SearchTerm}%") ||
					 EF.Functions.ILike(au.LastName, $"%{paginationRequest.SearchTerm}%") ||
					 EF.Functions.ILike(au.Email, $"%{paginationRequest.SearchTerm}%")));


		var totalRecords = await usersQuery.CountAsync(cancellationToken);

		var users = await usersQuery
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(au => new UsersDTO(
					au.Id,
					au.Email,
					au.FirstName,
					au.MiddleName!,
					au.LastName))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<UsersDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  users
			);
	}
	public async Task<PaginatedResult<ApplicationsDTO>> SearchApplicationsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var applicationsQuery = _dbcontext.AuthApplications
				.Where(au => au.IsActive &&
					(EF.Functions.ILike(au.AppName, $"%{paginationRequest.SearchTerm}%") ||
					 EF.Functions.ILike(au.Description!, $"%{paginationRequest.SearchTerm}%")));

		var totalRecords = await applicationsQuery.CountAsync(cancellationToken);

		var applications = await applicationsQuery
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(asm => new ApplicationsDTO(
					asm.AppId,
					asm.AppName,
					asm.Description!))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<ApplicationsDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  applications
			);
	}

	public async Task<Authusers> GetRawUserAsync(Guid id)
	{
		return await _dbcontext.AuthUsers
					 .Where(au => au.Id == id && au.IsActive)
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
							  where authRefreshToken.UserId == userId &&
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
					 .OrderByDescending(ov => ov.CreatedAt)
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

	public async Task<ApplicationsDTO?> GetApplicationAsync(int applicationId)
	{
		var application = await _dbcontext.AuthApplications.AsNoTracking()
		.Where(a => a.AppId == applicationId)
		 .Select(a => new ApplicationsDTO(
			a.AppId,
			a.AppName,
			a.Description!
		))
		.FirstOrDefaultAsync();

		return application;
	}

	public async Task<bool> DeleteApplicationAsync(int applicationId)
	{
		var application = await GetApplicationAsync(applicationId);
		if (application != null)
		{
			return false;
		}
		return true;
	}

	public async Task<bool> AddApplicationAsync(ApplicationsDTO applications)
	{
		var addedApplication = new AuthApplication
		{
			AppName = applications.applicationName,
			Description = applications.Description
		};
		var isAdded = await _dbcontext.AuthApplications.AddAsync(addedApplication);
		return true;
	}
}