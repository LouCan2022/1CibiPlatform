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
			.AuthApplications
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
	public async Task<PaginatedResult<SubMenusDTO>> GetSubMenusAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var totalRecords = await _dbcontext
			.AuthSubmenu
			.Where(asm => asm.IsActive)
			.LongCountAsync(cancellationToken);

		var subMenus = await _dbcontext.AuthSubmenu
			.Where(asm => asm.IsActive)
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(asm => new SubMenusDTO(
					asm.SubMenuId,
					asm.SubMenuName,
					asm.Description!))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<SubMenusDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  subMenus
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
	public async Task<PaginatedResult<SubMenusDTO>> SearchSubMenusAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var subMenusQuery = _dbcontext.AuthSubmenu
				.Where(asm => asm.IsActive &&
					(EF.Functions.ILike(asm.SubMenuName, $"%{paginationRequest.SearchTerm}%") ||
					 EF.Functions.ILike(asm.Description!, $"%{paginationRequest.SearchTerm}%")));

		var totalRecords = await subMenusQuery.CountAsync(cancellationToken);

		var subMenus = await subMenusQuery
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(asm => new SubMenusDTO(
					asm.SubMenuId,
					asm.SubMenuName,
					asm.Description!))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<SubMenusDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  subMenus
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
			.OrderByDescending(rt => rt.CreatedAt)
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

	public async Task<AuthApplication> GetApplicationAsync(int applicationId)
	{
		var application = await _dbcontext.AuthApplications
		.FirstOrDefaultAsync(x => x.AppId == applicationId);

		return application!;
	}
	public async Task<AuthSubMenu> GetSubMenuAsync(int subMenuId)
	{
		var subMenu = await _dbcontext.AuthSubmenu
		.FirstOrDefaultAsync(x => x.SubMenuId == subMenuId);

		return subMenu!;
	}

	public async Task<bool> DeleteApplicationAsync(AuthApplication application)
	{

		var isDeleted = _dbcontext.AuthApplications.Remove(application);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<bool> AddApplicationAsync(AddApplicationDTO applications)
	{
		var addedApplication = new AuthApplication
		{
			AppName = applications.AppName!,
			Description = applications.Description,
			IsActive = applications.IsActive,
			CreatedAt = DateTime.UtcNow
		};
		var isAdded = await _dbcontext.AuthApplications.AddAsync(addedApplication);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<AuthApplication> EditApplicationAsync(AuthApplication application)
	{
		_dbcontext.AuthApplications.Update(application);
		await _dbcontext.SaveChangesAsync();

		return application;
	}

	public async Task<bool> AddSubMenuAsync(AddSubMenuDTO subMenu)
	{
		var authSubMenu = new AuthSubMenu
		{
			SubMenuName = subMenu.SubMenuName!,
			Description = subMenu.Description,
			IsActive = subMenu.IsActive,
			CreatedAt = DateTime.UtcNow
		};
		var isAdded = await _dbcontext.AuthSubmenu.AddAsync(authSubMenu);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<bool> DeleteSubMenuAsync(AuthSubMenu subMenu)
	{
		var isDeleted = _dbcontext.AuthSubmenu.Remove(subMenu);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<AuthSubMenu> EditSubMenuAsync(AuthSubMenu subMenu)
	{
		_dbcontext.AuthSubmenu.Update(subMenu);
		await _dbcontext.SaveChangesAsync();

		return subMenu;
	}

	public async Task<PaginatedResult<AppSubRolesDTO>> GetAppSubRolesAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var totalRecords = await _dbcontext
			.AuthUserAppRoles
			.LongCountAsync(cancellationToken);

		var applications = await _dbcontext.AuthUserAppRoles
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(asr => new AppSubRolesDTO(
					asr.AppId,
					asr.UserId,
					asr.AppId,
					asr.Submenu,
					asr.RoleId))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<AppSubRolesDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  applications
			);
	}

	public async Task<PaginatedResult<AppSubRolesDTO>> SearchAppSubRoleAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var appSubRolesQuery = _dbcontext.AuthUserAppRoles
				.Where(asr => 
					(EF.Functions.ILike(asr.RoleId.ToString(), $"%{paginationRequest.SearchTerm}%") ||
					 EF.Functions.ILike(asr.Submenu.ToString()!, $"%{paginationRequest.SearchTerm}%") ||
					  EF.Functions.ILike(asr.AppId.ToString()!, $"%{paginationRequest.SearchTerm}%")));

		var totalRecords = await appSubRolesQuery.CountAsync(cancellationToken);

		var appSubRoles = await appSubRolesQuery
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(asr => new AppSubRolesDTO(
					asr.AppRoleId,
					asr.UserId,
					asr.AppId,
					asr.Submenu,
					asr.RoleId))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<AppSubRolesDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  appSubRoles
			);
	}

	public async Task<AuthUserAppRole> GetAppSubRoleAsync(int appSubRoleId)
	{
		var appSubRole = await _dbcontext.AuthUserAppRoles
		.FirstOrDefaultAsync(x => x.AppRoleId == appSubRoleId);

		return appSubRole!;
	}

	public async Task<bool> AddAppSubRoleAsync(AddAppSubRoleDTO appSubRole)
	{
		var authUserAppRole = new AuthUserAppRole
		{
			UserId = appSubRole.UserId!,
			AppId = appSubRole.AppId,
			Submenu = appSubRole.SubMenuId,
			RoleId = appSubRole.RoleId,
			AssignedBy = appSubRole.AssignedBy,
			AssignedAt = DateTime.UtcNow,
		};
		var isAdded = await _dbcontext.AuthUserAppRoles.AddAsync(authUserAppRole);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<bool> DeleteAppSubRoleAsync(AuthUserAppRole appSubRole)
	{
		var isDeleted = _dbcontext.AuthUserAppRoles.Remove(appSubRole);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<AuthUserAppRole> EditAppSubRoleAsync(AuthUserAppRole appSubRole)
	{
		_dbcontext.AuthUserAppRoles.Update(appSubRole);
		await _dbcontext.SaveChangesAsync();
		return appSubRole;
	}

	public async Task<PaginatedResult<RolesDTO>> GetRolesAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{
		var totalRecords = await _dbcontext
			.AuthRoles
			.LongCountAsync(cancellationToken);

		var roles = await _dbcontext.AuthRoles
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(asr => new RolesDTO(
					asr.RoleId,
					asr.RoleName,
					asr.Description!))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<RolesDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  roles
			);
	}

	public async Task<PaginatedResult<RolesDTO>> SearchRoleAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
	{

		var rolesQuery = _dbcontext.AuthRoles
				.Where(ar =>
					(EF.Functions.ILike(ar.RoleName, $"%{paginationRequest.SearchTerm}%") ||
					 EF.Functions.ILike(ar.Description!, $"%{paginationRequest.SearchTerm}%")));

		var totalRecords = await rolesQuery.CountAsync(cancellationToken);

		var roles = await rolesQuery
			.Skip((paginationRequest.PageIndex - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.AsNoTracking()
			.Select(ar => new RolesDTO(
					ar.RoleId,
					ar.RoleName,
					ar.Description!))
			.ToListAsync(cancellationToken);

		return new PaginatedResult<RolesDTO>
			(
			  paginationRequest.PageIndex,
			  paginationRequest.PageSize,
			  totalRecords,
			  roles
			);
	}

	public async Task<bool> AddRoleAsync(AddRoleDTO role)
	{
		var authRole = new AuthRole
		{
			RoleName = role.RoleName!,
			Description = role.Description,
			CreatedAt = DateTime.UtcNow,
		};
		var isAdded = await _dbcontext.AuthRoles.AddAsync(authRole);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<bool> DeleteRoleAsync(AuthRole role)
	{
		var isDeleted = _dbcontext.AuthRoles.Remove(role);
		await _dbcontext.SaveChangesAsync();
		return true;
	}

	public async Task<AuthRole> EditRoleAsync(AuthRole role)
	{
		_dbcontext.AuthRoles.Update(role);
		await _dbcontext.SaveChangesAsync();
		return role;
	}
	public async Task<AuthRole> GetRoleAsync(int roleId)
	{
		var role = await _dbcontext.AuthRoles
		.FirstOrDefaultAsync(x => x.RoleId == roleId);

		return role!;
	}
}