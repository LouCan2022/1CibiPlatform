namespace SSO.Features.LoginCallback;

public record SSOLoginCallbackCommand(string ReturnUrl = "/") : ICommand<SSOLoginCallbackResult>;

public record SSOLoginCallbackResult(SSOLoginResponseDTO result);

public class SSOLoginCallbackHandler : ICommandHandler<SSOLoginCallbackCommand, SSOLoginCallbackResult>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _config;
	private readonly ILogger<SSOLoginCallbackHandler> _logger;
	private readonly string _signinScheme;
	private readonly bool _isHttps;
	private readonly IConfiguration _configuration;
	private readonly int _cookieExpiryinDaysKey;
	private readonly string _userEmailCookieName;

	public SSOLoginCallbackHandler(
		IHttpContextAccessor httpContextAccessor,
		ILogger<SSOLoginCallbackHandler> logger,
		IConfiguration configuration)
	{
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
		_configuration = configuration;
		_signinScheme = _configuration!.GetValue<string>("SSOMetadata:SigninScheme")!;
		_isHttps = _configuration!.GetValue<bool>("AuthWeb:IsHttps");
		_cookieExpiryinDaysKey = _configuration!.GetValue<int>("AuthWeb:CookieExpiryInDayIsRememberMe");
		_userEmailCookieName = _configuration!.GetValue<string>("SSOMetadata:UserEmailCookieName") ?? "";
	}

	public async Task<SSOLoginCallbackResult> Handle(SSOLoginCallbackCommand request, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		// Authenticate using the temporary SAML cookie
		AuthenticateResult result = await httpContext!.AuthenticateAsync(_signinScheme);

		if (!result.Succeeded)
		{
			_logger.LogError("SAML authentication failed");
			throw new Exception("Authentication failed");
		}

		// Extract claims
		var claimCollection = result?.Principal?.Claims != null
			? new Collection<Claim>(result.Principal.Claims.ToList())
			: new Collection<Claim>();

		if (claimCollection.Count == 0)
		{
			_logger.LogError("No claims found in SAML response");
			throw new Exception("No claims found");
		}

		// Get user information
		var nameIdentifier = claimCollection.FirstOrDefault(c =>
			c.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value;
		var email = claimCollection.FirstOrDefault(c =>
			c.Type.Equals(ClaimTypes.Email, StringComparison.OrdinalIgnoreCase))?.Value;
		var name = claimCollection.FirstOrDefault(c =>
			c.Type.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))?.Value;

		_logger.LogInformation($"User authenticated via SAML: {email}");

		// CREATE NEW IDENTITY with the cookie scheme authentication type
		var cookieIdentity = new ClaimsIdentity(claimCollection, _signinScheme);
		var cookiePrincipal = new ClaimsPrincipal(cookieIdentity);

		//Create httpCookie only for emal
		var emailCookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = _isHttps,
			SameSite = SameSiteMode.Lax,
			Expires = DateTimeOffset.UtcNow.AddDays(_cookieExpiryinDaysKey)
		};

		// Sign in with the NEW principal that has the correct authentication type
		await httpContext!.SignInAsync(
			_signinScheme,
			cookiePrincipal,
			new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_cookieExpiryinDaysKey),
				AllowRefresh = true
			});

		//Set email cookie	
		_httpContextAccessor.HttpContext!.Response.Cookies.Append(_userEmailCookieName, nameIdentifier ?? string.Empty, emailCookieOptions);

		return new SSOLoginCallbackResult(new SSOLoginResponseDTO(nameIdentifier!, name!, true));
	}
}
