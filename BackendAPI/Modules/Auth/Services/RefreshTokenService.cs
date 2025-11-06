namespace Auth.Services
{
	public class RefreshTokenService : IRefreshTokenService
	{
		private readonly IAuthRepository _authRepository;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IJWTService _jWTService;
		private readonly IConfiguration _configuration;
		private readonly ILogger<RefreshTokenService> _logger;

		private readonly string _httpCookieOnlyKey;
		private readonly double _expiryinMinutesKey;
		private readonly string _refreshTokenKey;
		private readonly bool _isHttps;
		private readonly int _cookieExpiryinDaysKey;

		public RefreshTokenService(
			IAuthRepository authRepository,
			IHttpContextAccessor httpContextAccessor,
			IJWTService jWTService,
			IConfiguration configuration,
			ILogger<RefreshTokenService> logger)
		{
			this._authRepository = authRepository;
			this._httpContextAccessor = httpContextAccessor;
			this._jWTService = jWTService;
			this._configuration = configuration;
			this._logger = logger;

			_httpCookieOnlyKey = _configuration.GetValue<string>("HttpCookieOnlyKey") ?? "";
			_expiryinMinutesKey = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value! ?? "");
			_refreshTokenKey = _configuration.GetSection("AuthWeb:AuthWebHttpCookieOnlyKey").Value! ?? "";
			_isHttps = bool.Parse(_configuration.GetSection("AuthWeb:isHttps").Value!);
			_cookieExpiryinDaysKey = _configuration.GetValue<int>("AuthWeb:CookieExpiryInDayIsRememberMe");
		}


		public virtual (string, string) GenerateRefreshToken()
		{
			// Generate random token
			var randomNumber = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);


			var token = Convert.ToBase64String(randomNumber)
				.TrimEnd('=')
				.Replace('+', '-')
				.Replace('/', '_');

			// Hash for storage
			var hashedToken = HashToken(token);
			return (token, hashedToken);
		}

		public virtual string HashToken(string token)
		{
			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
			return Convert.ToBase64String(hashBytes);
		}

		public virtual bool ValidateHashToken(string providedToken, string storedHash)
		{
			var decodedToken = System.Net.WebUtility.UrlDecode(providedToken);

			var providedHash = HashToken(decodedToken);

			return CryptographicOperations.FixedTimeEquals(
				Convert.FromBase64String(providedHash),
				Convert.FromBase64String(storedHash)
			);
		}

		public virtual Task<string> RevokeTokenAsync()
		{
			throw new NotImplementedException();
		}

		public virtual async Task<LoginResponseWebDTO> GetNewAccessTokenAsync(Guid userId, string refreshToken)
		{
			_logger.LogInformation("Attempting to get new access token using refresh token.");

			var hashToken = HashToken(refreshToken);

			var userData = await _authRepository.GetNewUserDataAsync(userId);

			// checking if client credentials are valid
			if (userData == null)
			{
				_logger.LogWarning("Refresh Token is not found or invalid.");
				throw new NotFoundException("Refresh Token is not found.");

			}

			if (!ValidateHashToken(refreshToken, userData.refreshToken))
			{
				_logger.LogWarning("Invalid refresh token provided for user ID: {UserId}", userId);
				throw new UnauthorizedAccessException("Invalid refresh token.");
			}

			var roleId = userData.roleId;
			var appId = userData.AppId;
			var subMenuId = userData.SubMenuId;


			// produce access token
			var loginDTO = userData.Adapt<LoginDTO>();
			_logger.LogInformation("Generating JWT token for user: {Email}", userData.Email);
			string jwtToken = this._jWTService.GetAccessToken(loginDTO);
			SetAccessTokenCookie(jwtToken);

			// reuse refresh token
			var (newRefreshToken, newRefreshTokenHash) = this.GenerateRefreshToken();
			SetRefreshTokenCookie(newRefreshToken, false);

			_logger.LogInformation("Creating new refresh token for user: {Email}", userData.Email);
			// reuse existing refresh token if not expired
			return new LoginResponseWebDTO(
				userData.Id.ToString()!,
				jwtToken,
				newRefreshToken!,
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
				SameSite = SameSiteMode.None,
				Expires = DateTime.UtcNow.AddMinutes(_expiryinMinutesKey)
			};


			_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey!, accessToken, cookieAccessTokenOptions);
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


			_httpContextAccessor.HttpContext!.Response.Cookies.Append(_refreshTokenKey!, refreshToken, cookieRefreshTokenOptions);
		}

		protected virtual int ExpireInMinutes()
		{
			double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

			var expireIn = (int)(configTime * 60);

			return expireIn;
		}
	}
}
