namespace Auth.Features.Login;

public record LoginCommand(LoginCred LoginCred) : ICommand<LoginResult>;

public record LoginResult(LoginResponseDTO loginResponseDTO);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
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

public class LoginHandler : ICommandHandler<LoginCommand, LoginResult>
{
	private readonly IAuthRepository _authRepository;
	private readonly IPasswordHasherService _passwordHasherService;
	private readonly IConfiguration _configuration;
	private readonly IJWTService _jWTService;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<LoginHandler> _logger;

	public LoginHandler(
		IAuthRepository authRepository,
		IPasswordHasherService passwordHasherService,
		IConfiguration configuration,
		IJWTService jWTService,
		IHttpContextAccessor httpContextAccessor,
		ILogger<LoginHandler> logger)
	{
		this._authRepository = authRepository;
		this._passwordHasherService = passwordHasherService;
		this._configuration = configuration;
		this._jWTService = jWTService;
		this._httpContextAccessor = httpContextAccessor;
		this._logger = logger;
	}

	public async Task<LoginResult> Handle(
		LoginCommand request,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Login attempt for user: {Username}", request.LoginCred.Username);
		// mapping LoginCommand to LoginCred
		LoginCred cred = request.LoginCred.Adapt<LoginCred>();

		// fetching user data from database
		LoginDTO userData = await this._authRepository.LoginAsync(cred);

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

		// produce JWT token
		string jwtToken = this._jWTService.GetAccessToken(userData);
		double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

		// set cookie key
		var _httpCookieOnlyKey = _configuration.GetValue<string>("HttpCookieOnlyKey");

		// set httpcookieonly
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = false, // Only send over HTTPS
			SameSite = SameSiteMode.Strict,
			Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(configTime))
		};

		_httpContextAccessor.HttpContext!.Response.Cookies.Append(_httpCookieOnlyKey, jwtToken, cookieOptions);

		_logger.LogInformation("Login successful for user: {Username}", request.LoginCred.Username);

		var response = new LoginResponseDTO(
			userData.Id.ToString()!,
			jwtToken,
			"bearer",
			ExpireInMinutes(),
			"admin",
			DateTime.Now.ToString(),
			DateTime.Now.AddMinutes(configTime).ToString()
		);

		return new LoginResult(response);
	}


	protected virtual int ExpireInMinutes()
	{
		double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

		var expireIn = (int)(configTime * 60);

		return expireIn;
	}
}



