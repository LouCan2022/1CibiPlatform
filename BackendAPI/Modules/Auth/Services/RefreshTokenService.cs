namespace Auth.Services
{
	public class RefreshTokenService : IRefreshTokenService
	{
		private readonly IAuthRepository _authRepository;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IJWTService _jWTService;
		private readonly IConfiguration _configuration;
		private readonly ILogger<RefreshTokenService> _logger;

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
		}


		public (string, string) GenerateRefreshToken()
		{
			// Generate random token
			var randomNumber = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			var token = Convert.ToBase64String(randomNumber);

			// Hash for storage
			var hashedToken = HashToken(token);

			return (token, hashedToken);
		}


		private string HashToken(string token)
		{
			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
			return Convert.ToBase64String(hashBytes);
		}

		public bool ValidateHashToken(string providedToken, string storedHash)
		{
			var providedHash = HashToken(providedToken);
			return CryptographicOperations.FixedTimeEquals(
				Convert.FromBase64String(providedHash),
				Convert.FromBase64String(storedHash)
			);
		}

		public Task<string> RevokeToken()
		{
			throw new NotImplementedException();
		}

		public async Task<LoginResponseWebDTO> GetNewAccessToken(string refreshToken)
		{
			var userData = await _authRepository.GetNewUserDataAsync(refreshToken);

			// checking if client credentials are valid
			if (userData == null)
			{
				_logger.LogWarning("Refresh Token is not found or invalid.");
				throw new NotFoundException("Refresh Token is not found");

			}

			var roleId = userData.roleId;
			var appId = userData.AppId;
			var subMenuId = userData.SubMenuId;


			// produce token
			string jwtToken = this._jWTService.GetAccessToken(userData);
			(string newRefreshToken, string hashRefreshToken) = this.GenerateRefreshToken();

			double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

			// set cookie key
			var _httpCookieOnlyKey = _configuration.GetValue<string>("HttpCookieOnlyKey");

			// set httpcookieonly
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = false, // Only send over HTTPS
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddDays(configTime)
			};

			_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey, jwtToken, cookieOptions);

			_logger.LogInformation("Generated new JWT token for user: {UserId}", userData.Id);


			// save refresh token to database
			_logger.LogInformation("Saving refresh token for user: {UserId}", userData.Id);
			await this._authRepository.SaveRefreshTokenAsync(userData.Id, hashRefreshToken, DateTime.UtcNow.AddMinutes(configTime));

			//Revoke old refresh token
			_logger.LogInformation("Revoking old refresh token for user: {UserId}", userData.Id);
			await this._authRepository.UpdateRevokeReasonAsync(refreshToken, "Replaced by new token");

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
				DateTime.Now.AddMinutes(configTime).ToString());

		}
		protected virtual int ExpireInMinutes()
		{
			double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

			var expireIn = (int)(configTime * 60);

			return expireIn;
		}
	}
}
