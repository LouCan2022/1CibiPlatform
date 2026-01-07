namespace Auth.Services;

public class LoginService : ILoginService
{
	private readonly IAuthRepository _authRepository;
	private readonly IPasswordHasherService _passwordHasherService;
	private readonly IConfiguration _configuration;
	private readonly IJWTService _jWTService;
	private readonly IRefreshTokenService _refreshTokenService;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<LoginService> _logger;
	private readonly string _httpCookieOnlyKey;
	private readonly double _expiryinMinutesKey;
	private readonly string _httpCookieOnlyRefreshTokenKey;
	private readonly int _cookieExpiryinDaysKey;
	private readonly bool _isHttps;
	private readonly string _isUserLoginTag = "is_user_login";

	public LoginService(
	IAuthRepository authRepository,
	IPasswordHasherService passwordHasherService,
	IConfiguration configuration,
	IJWTService jWTService,
	IRefreshTokenService refreshTokenService,
	IHttpContextAccessor httpContextAccessor,
	ILogger<LoginService> logger)
	{
		this._authRepository = authRepository;
		this._passwordHasherService = passwordHasherService;
		this._configuration = configuration;
		this._jWTService = jWTService;
		this._refreshTokenService = refreshTokenService;
		this._httpContextAccessor = httpContextAccessor;
		this._logger = logger;

		_httpCookieOnlyKey = _configuration.GetValue<string>("HttpCookieOnlyKey") ?? "";
		_expiryinMinutesKey = _configuration.GetValue<double>("Jwt:ExpiryInMinutes");
		_httpCookieOnlyRefreshTokenKey = _configuration.GetValue<string>("AuthWeb:AuthWebHttpCookieOnlyKey") ?? "";
		_cookieExpiryinDaysKey = _configuration.GetValue<int>("AuthWeb:CookieExpiryInDayIsRememberMe");
		_isHttps = _configuration.GetValue<bool>("AuthWeb:isHttps");
	}


	public async Task<LoginResponseDTO> LoginAsync(string username, string password)
	{
		var logContext = new
		{
			Action = "LoggingToApi",
			Step = "StartLogin",
			Email = username,
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Login attempt for user: {@Context}", logContext);

		var loginCred = new LoginWebCred(username, password, false);

		// fetching user data from database
		LoginDTO userData = await this._authRepository.GetUserDataAsync(loginCred);

		// checking if client credentials are valid
		if (userData == null)
		{
			_logger.LogWarning("Login failed: Invalid username or password for user: {@Context}", logContext);
			throw new NotFoundException("Invalid username or password.");
		}

		// verifying password
		bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, password);

		if (!isPasswordValid)
		{
			_logger.LogWarning("Login failed: Invalid password for user: {@Context}", logContext);
			throw new NotFoundException("Invalid username or password.");
		}

		var IsApprove = userData.IsApproved;

		if (IsApprove == false)
		{
			_logger.LogInformation("User application and role data retrieved for user: {@Context}", logContext);
			throw new UnauthorizedAccessException("Your account has not been approved yet. Please contact an administrator for assistance.");
		}

		// produce JWT token
		string jwtToken = this._jWTService.GetAccessToken(userData);

		// set httpcookieonly
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = _isHttps,
			SameSite = SameSiteMode.Lax,
			Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_expiryinMinutesKey))
		};

		_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey!, jwtToken, cookieOptions);

		_logger.LogInformation("Login successful for user: {@Context}", logContext);

		var name = !string.IsNullOrEmpty(userData.MiddleName) ?
			$"{userData.FirstName} {userData.MiddleName} {userData.LastName}" :
			$"{userData.FirstName} {userData.LastName}";

		var response = new LoginResponseDTO(
			userData.Id.ToString()!,
			jwtToken,
			"bearer",
			ExpireInMinutes(),
			name,
			userData.Email,
			DateTime.Now.ToString(),
			DateTime.Now.AddMinutes(_expiryinMinutesKey).ToString()
		);

		return response;
	}

	public async Task<LoginResponseWebDTO> LoginWebAsync(LoginWebCred cred)
	{

		var logContext = new
		{
			Action = "LoggingToWeb",
			Step = "StartLogin",
			Email = cred.Username,
			IsRememberMe = cred.IsRememberMe,
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Login attempt for user: {@Context}", logContext);

		// fetching user data from database
		LoginDTO userData = await this._authRepository.GetUserDataAsync(cred);

		// checking if client credentials are valid
		if (userData == null)
		{
			// invalid Refresh TOKEN
			_logger.LogWarning("Login failed: Invalid username or password for user: {@Context}", logContext);
			throw new NotFoundException("Invalid username or password");
		}

		// verifying password
		bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, cred.Password);

		if (!isPasswordValid)
		{
			_logger.LogWarning("Login failed: Invalid password for user: {@Context}", logContext);
			throw new NotFoundException("Invalid username or password.");
		}

		// produce refresh token
		var refreshTokenExist = this.GetRefreshTokenFromCookie();
		var roleId = userData.roleId;
		var appId = userData.AppId;
		var subMenuId = userData.SubMenuId;
		var IsApprove = userData.IsApproved;

		if (IsApprove == false)
		{
			_logger.LogInformation("User approval status retrieved for user: {@Context}", logContext);
			throw new UnauthorizedAccessException("Your account has not been approved yet. Please contact an administrator for assistance.");
		}
		if (!appId.Any() ||
			!subMenuId.Any() ||
			!roleId.Any())
		{
			_logger.LogInformation("User application and role data retrieved for user: {@Context}", logContext);
			throw new UnauthorizedAccessException("Your account has no assigned application. Please contact an administrator for assistance.");
		}

		// produce access token
		string jwtToken = this._jWTService.GetAccessToken(userData);
		SetAccessTokenCookie(jwtToken);


		var name = !string.IsNullOrEmpty(userData.MiddleName) ?
		$"{userData.FirstName} {userData.MiddleName} {userData.LastName}" :
		$"{userData.FirstName} {userData.LastName}";



		var successContext = new
		{
			Action = "LoggingToWeb",
			Step = "StartLogin",
			Email = cred.Username,
			Userid = userData.Id,
			IsRememberMe = cred.IsRememberMe,
			Timestamp = DateTime.UtcNow
		};


		if (refreshTokenExist != null)
		{
			_logger.LogInformation("Login successful for user: {@Context}", successContext);

			// reuse existing refresh token if not expired
			return new LoginResponseWebDTO(
				userData.Id.ToString()!,
				jwtToken,
				refreshTokenExist,
				name,
				"bearer",
				ExpireInMinutes(),
				appId,
				subMenuId,
				roleId,
				DateTime.Now.ToString(),
				DateTime.Now.AddMinutes(_expiryinMinutesKey).ToString()
			);
		}

		// generate new refresh token
		(string refreshToken, string hashRefreshToken) = this._refreshTokenService.GenerateRefreshToken();
		SetRefreshTokenCookie(refreshToken, cred.IsRememberMe);

		// save refresh token to database
		// save if http cookie only for refresh token is already expired
		await this._authRepository.SaveRefreshTokenAsync(userData.Id, hashRefreshToken, DateTime.UtcNow.AddMinutes(_expiryinMinutesKey));

		_logger.LogInformation("Login successful for user: {@Context}", successContext);

		return new LoginResponseWebDTO(
			userData.Id.ToString()!,
			jwtToken,
			refreshToken,
			name,
			"bearer",
			ExpireInMinutes(),
			appId,
			subMenuId,
			roleId,
			DateTime.Now.ToString(),
			DateTime.Now.AddMinutes(_expiryinMinutesKey).ToString()
		);
	}

	protected virtual void SetAccessTokenCookie(
		string accessToken)
	{
		// set httpcookieonly
		var cookieAccessTokenOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = _isHttps,
			SameSite = SameSiteMode.Lax,
			Expires = DateTime.UtcNow.AddMinutes(_expiryinMinutesKey)
		};


		_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey!, accessToken, cookieAccessTokenOptions);
	}

	protected virtual string? GetAccessTokenFromCookie()
	{
		var accessToken = _httpContextAccessor.HttpContext!.Request.Cookies[_httpCookieOnlyKey!];
		return accessToken;
	}

	protected virtual string? GetRefreshTokenFromCookie()
	{
		var accessToken = _httpContextAccessor.HttpContext!.Request.Cookies[_httpCookieOnlyRefreshTokenKey!];
		return accessToken;
	}


	protected virtual void SetRefreshTokenCookie(
	string refreshToken,
	bool isRememberMe)
	{
		// set httpcookieonly

		var cookieRefreshTokenOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = _isHttps,
			SameSite = SameSiteMode.Lax,
			Expires = isRememberMe ? DateTime.UtcNow.AddDays(_cookieExpiryinDaysKey) : DateTime.UtcNow.AddMinutes(Convert.ToInt32(_expiryinMinutesKey))
		};


		_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyRefreshTokenKey!, refreshToken, cookieRefreshTokenOptions);
	}

	protected virtual bool RemoveAccessAndRefreshTokenCookie()
	{
		_httpContextAccessor.HttpContext!.Response.Cookies.Delete(_httpCookieOnlyKey!);
		_httpContextAccessor.HttpContext!.Response.Cookies.Delete(_httpCookieOnlyRefreshTokenKey!);
		return true;
	}

	protected virtual int ExpireInMinutes()
	{

		var expireIn = (int)(_expiryinMinutesKey * 60);

		return expireIn;
	}

	public async Task<bool> LogoutAsync(
		Guid userId,
		string revokeReason)
	{
		var logContext = new
		{
			Action = "LogoutUser",
			Step = "StartLogout",
			UserId = userId,
			RevokeReason = revokeReason,
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Logout attempt for user: {@Context}", logContext);

		var logoutCachekey = $"{_isUserLoginTag}_{GetRefreshTokenFromCookie()}";

		if (string.IsNullOrEmpty(GetRefreshTokenFromCookie()))
		{
			_logger.LogWarning("Logout failed: No refresh token found in cookies for user: {@Context}", logContext);
			throw new BadRequestException("Logout failed.");
		}


		var userData = await this._authRepository.IsUserExistAsync(userId);


		if (userData == null)
		{
			_logger.LogWarning("Logout failed: User not found for user: {@Context}", logContext);
			throw new NotFoundException("User not found.");
		}

		foreach (var item in userData)
		{
			if (_refreshTokenService.ValidateHashToken(GetRefreshTokenFromCookie()!, item.TokenHash))
			{
				var result = await _authRepository.UpdateRevokeReasonAsync(item, revokeReason);

				if (result == false)
				{
					_logger.LogInformation("Logout failed for user: {@Context}", logContext);
					throw new BadRequestException("Logout failed.");
				}

				this.RemoveAccessAndRefreshTokenCookie();

				_logger.LogInformation("Logout successful for user: {@Context}", logContext);

				return result;
			}
		}

		_logger.LogWarning("Logout failed: Refresh token not found for user: {@Context}", logContext);
		throw new NotFoundException("User not found.");
	}

	public async Task<bool> IsAuthenticated()
	{
		var cachekey = $"{_isUserLoginTag}_{GetRefreshTokenFromCookie()}";

		var logContext = new
		{
			Action = "AuthenticateUser",
			Step = "StartAuthentication",
			RefreshToken = GetRefreshTokenFromCookie(),
			RequestId = Guid.NewGuid(),
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Checking authentication status... {@Context}", logContext);

		if (string.IsNullOrEmpty(GetRefreshTokenFromCookie()))
		{
			_logger.LogWarning("Authentication check failed: No refresh token found in cookies.");
			return false;
		}

		_logger.LogInformation("User is authenticated for {@Context}", logContext);

		return true;
	}
}

