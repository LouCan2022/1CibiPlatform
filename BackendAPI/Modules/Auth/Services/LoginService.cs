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
		_expiryinMinutesKey = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);
	}


	public async Task<LoginResponseDTO> LoginAsync(LoginCred cred)
	{
		_logger.LogInformation("Login attempt for user: {Username}", cred.Username);


		// fetching user data from database
		LoginDTO userData = await this._authRepository.GetUserDataAsync(cred);

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
			Secure = false, // Only send over HTTPS
			SameSite = SameSiteMode.Strict,
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

	public async Task<LoginResponseWebDTO> LoginWebAsync(LoginCred cred)
	{
		_logger.LogInformation("Login attempt for user: {Username}", cred.Username);

		// fetching user data from database
		LoginDTO userData = await this._authRepository.GetUserDataAsync(cred);


		var roleId = userData.roleId;
		var appId = userData.AppId;
		var subMenuId = userData.SubMenuId;

		// checking if client credentials are valid
		if (userData == null)
		{
			// invalid Refresh TOKEN
			_logger.LogWarning("Login failed: Invalid username or password for user: {Username}", cred.Username);
			throw new NotFoundException("Refresh Token not found");
		}

		// verifying password
		bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, cred.Password);

		if (!isPasswordValid)
		{
			_logger.LogWarning("Login failed: Invalid password for user: {Username}", cred.Username);
			throw new NotFoundException("Invalid username or password.");
		}

		// produce token
		string jwtToken = this._jWTService.GetAccessToken(userData);
		(string refreshToken, string hashRefreshToken) = this._refreshTokenService.GenerateRefreshToken();


		// set httpcookieonly
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = false, // Only send over HTTPS
			SameSite = SameSiteMode.Strict,
			Expires = DateTime.UtcNow.AddMinutes(_expiryinMinutesKey)
		};

		_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey!, jwtToken, cookieOptions);

		_logger.LogInformation("Login successful for user: {Username}", cred.Username);


		// save refresh token to database
		_logger.LogInformation("Saving refresh token for user: {UserId}", userData.Id);
		await this._authRepository.SaveRefreshTokenAsync(userData.Id, hashRefreshToken, DateTime.UtcNow.AddMinutes(_expiryinMinutesKey));


		return new LoginResponseWebDTO(
			userData.Id.ToString()!,
			jwtToken,
			refreshToken,
			"bearer",
			ExpireInMinutes(),
			userData.Username,
			appId,
			subMenuId,
			roleId,
			DateTime.Now.ToString(),
			DateTime.Now.AddMinutes(_expiryinMinutesKey).ToString()
		);
	}


	protected virtual int ExpireInMinutes()
	{

		var expireIn = (int)(_expiryinMinutesKey * 60);

		return expireIn;
	}
}

