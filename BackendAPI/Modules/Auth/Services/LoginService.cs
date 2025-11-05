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


	public async Task<LoginResponseDTO> LoginAsync(LoginCred cred)
	{
		_logger.LogInformation("Login attempt for user: {Username}", cred.Username);

		var loginCred = new LoginWebCred(cred.Username, cred.Password, false);

		// fetching user data from database
		LoginDTO userData = await this._authRepository.GetUserDataAsync(loginCred);

		// checking if client credentials are valid
		if (userData == null)
		{
			_logger.LogWarning("Login failed: Invalid username or password for user: {Username}", cred.Username);
			throw new NotFoundException("Invalid username or password.");
		}

		// verifying password
		bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, cred.Password);

		if (!isPasswordValid)
		{
			_logger.LogWarning("Login failed: Invalid password for user: {Username}", cred.Username);
			throw new NotFoundException("Invalid username or password.");
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

		_logger.LogInformation("Login successful for user: {Username}", cred.Username);

		var response = new LoginResponseDTO(
			userData.Id.ToString()!,
			jwtToken,
			"bearer",
			ExpireInMinutes(),
			"admin",
			DateTime.Now.ToString(),
			DateTime.Now.AddMinutes(_expiryinMinutesKey).ToString()
		);

		return response;
	}

	public async Task<LoginResponseWebDTO> LoginWebAsync(LoginWebCred cred)
	{
		_logger.LogInformation("Login attempt for user: {Username}", cred.Username);

		// fetching user data from database
		LoginDTO userData = await this._authRepository.GetUserDataAsync(cred);

		// checking if client credentials are valid
		if (userData == null)
		{
			// invalid Refresh TOKEN
			_logger.LogWarning("Login failed: Invalid username or password for user: {Username}", cred.Username);
			throw new NotFoundException("Invalid username or password");
		}


		// verifying password
		bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, cred.Password);

		if (!isPasswordValid)
		{
			_logger.LogWarning("Login failed: Invalid password for user: {Username}", cred.Username);
			throw new NotFoundException("Invalid username or password.");
		}

		// produce access token
		_logger.LogInformation("Generating JWT token for user: {Username}", cred.Username);
		string jwtToken = this._jWTService.GetAccessToken(userData);
		SetAccessTokenCookie(jwtToken);


		// produce refresh token
		var refreshTokenExist = this.GetRefreshTokenFromCookie();

		var roleId = userData.roleId;
		var appId = userData.AppId;
		var subMenuId = userData.SubMenuId;

		if (refreshTokenExist != null)
		{
			_logger.LogInformation("Reusing existing refresh token for user: {Username}", cred.Username);
			// reuse existing refresh token if not expired
			return new LoginResponseWebDTO(
				userData.Id.ToString()!,
				jwtToken,
				refreshTokenExist,
				"bearer",
				ExpireInMinutes(),
				appId,
				subMenuId,
				roleId,
				DateTime.Now.ToString(),
				DateTime.Now.AddMinutes(_expiryinMinutesKey).ToString()
			);
		}

		_logger.LogInformation("Generating refresh token for user: {Username}", cred.Username);
		(string refreshToken, string hashRefreshToken) = this._refreshTokenService.GenerateRefreshToken();
		SetRefreshTokenCookie(refreshToken, cred.IsRememberMe);

		// save refresh token to database
		// save if http cookie only for refresh token is already expired
		_logger.LogInformation("Saving refresh token for user: {UserId}", userData.Id);
		await this._authRepository.SaveRefreshTokenAsync(userData.Id, hashRefreshToken, DateTime.UtcNow.AddMinutes(_expiryinMinutesKey));


		_logger.LogInformation("Login successful for user: {Username}", cred.Username);

		return new LoginResponseWebDTO(
			userData.Id.ToString()!,
			jwtToken,
			refreshToken,
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
		_logger.LogInformation("Logout attempt for user: {UserId}", userId);

		if (string.IsNullOrEmpty(GetRefreshTokenFromCookie()))
		{
			_logger.LogWarning("Logout failed: No refresh token found in cookies for user: {UserId}", userId);
			throw new BadRequestException("Logout failed.");
		}


		var userData = await this._authRepository.IsUserExistAsync(userId);

		if (userData == null)
		{
			_logger.LogWarning("Logout failed: User not found for user: {UserId}", userId);
			throw new NotFoundException("User not found.");
		}

		if (!_refreshTokenService.ValidateHashToken(GetRefreshTokenFromCookie()!, userData.TokenHash))
		{
			_logger.LogWarning("Logout failed: Invalid refresh token for user: {UserId}", userId);
			throw new BadRequestException("Logout failed.");
		}

		var result = await _authRepository.UpdateRevokeReasonAsync(userData, revokeReason);

		if (result == false)
		{
			_logger.LogInformation("Logout failed for user: {UserId}", userId);
			throw new BadRequestException("Logout failed.");
		}

		this.RemoveAccessAndRefreshTokenCookie();

		_logger.LogInformation("Logout successful for user: {UserId}", userId);

		return result;

	}

	public Task<bool> IsAuthenticated()
	{
		_logger.LogInformation("Checking authentication status...");

		if (string.IsNullOrEmpty(GetRefreshTokenFromCookie()))
		{
			_logger.LogWarning("Authentication check failed: No refresh token found in cookies.");
			return Task.FromResult(false);
		}

		_logger.LogInformation("User is authenticated.");
		return Task.FromResult(true);
	}
}

