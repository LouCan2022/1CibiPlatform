namespace Auth.Features.LoginWeb;

public class LoginWebHandler
{
	public record LoginWebCommand(LoginCred LoginCred) : ICommand<LoginWebResult>;

	public record LoginWebResult(LoginResponseWebDTO loginResponseWebDTO);

	public class LoginCommandValidator : AbstractValidator<LoginWebCommand>
	{
		public LoginCommandValidator()
		{
			RuleFor(x => x.LoginCred.Username)
				.NotEmpty().WithMessage("Username is required.")
				.MaximumLength(50).WithMessage("Username must not exceed 50 characters.");
			RuleFor(x => x.LoginCred.Password)
				.NotEmpty().WithMessage("Password is required.");
		}
	}

	public class LoginHandler : ICommandHandler<LoginWebCommand, LoginWebResult>
	{
		private readonly IAuthRepository _authRepository;
		private readonly IPasswordHasherService _passwordHasherService;
		private readonly IConfiguration _configuration;
		private readonly IJWTService _jWTService;
		private readonly IRefreshTokenService _refreshTokenService;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILogger<LoginHandler> _logger;

		public LoginHandler(
			IAuthRepository authRepository,
			IPasswordHasherService passwordHasherService,
			IConfiguration configuration,
			IJWTService jWTService,
			IRefreshTokenService refreshTokenService,
			IHttpContextAccessor httpContextAccessor,
			ILogger<LoginHandler> logger)
		{
			this._authRepository = authRepository;
			this._passwordHasherService = passwordHasherService;
			this._configuration = configuration;
			this._jWTService = jWTService;
			this._refreshTokenService = refreshTokenService;
			this._httpContextAccessor = httpContextAccessor;
			this._logger = logger;
		}

		public async Task<LoginWebResult> Handle(
			LoginWebCommand request,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation("Login attempt for user: {Username}", request.LoginCred.Username);
			// mapping LoginCommand to LoginCred
			LoginCred cred = request.LoginCred.Adapt<LoginCred>();

			// fetching user data from database
			LoginDTO userData = await this._authRepository.LoginAsync(cred);


			var roleId = userData.roleId;
			var appId = userData.AppId;
			var subMenuId = userData.SubMenuId;

			// checking if client credentials are valid
			if (userData == null)
			{
				_logger.LogWarning("Login failed: Invalid username or password for user: {Username}", request.LoginCred.Username);
				throw new NotFoundException("Invalid username or password.");
			}

			// verifying password
			bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, request.LoginCred.Password);

			if (!isPasswordValid)
			{
				_logger.LogWarning("Login failed: Invalid password for user: {Username}", request.LoginCred.Username);
				throw new NotFoundException("Invalid username or password.");
			}

			// produce token
			string jwtToken = this._jWTService.GetAccessToken(userData);
			(string refreshToken, string hashRefreshToken) = this._refreshTokenService.GenerateRefreshToken();

			double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

			// set cookie key
			var _httpCookieOnlyKey = _configuration.GetValue<string>("HttpCookieOnlyKey");

			// set httpcookieonly
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = false, // Only send over HTTPS
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.UtcNow.AddDays(1)
			};

			_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey, jwtToken, cookieOptions);

			_logger.LogInformation("Login successful for user: {Username}", request.LoginCred.Username);

			var response = new LoginResponseWebDTO(
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
				DateTime.Now.AddMinutes(configTime).ToString()
			);

			return new LoginWebResult(response);
		}


		protected virtual int ExpireInMinutes()
		{
			double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

			var expireIn = (int)(configTime * 60);

			return expireIn;
		}
	}
}
