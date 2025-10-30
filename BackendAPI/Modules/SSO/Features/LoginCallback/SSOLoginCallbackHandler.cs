namespace SSO.Features.LoginCallback;
public record SSOLoginCallbackCommand(string ReturnUrl = "/") : ICommand<SSOLoginCallbackResult>;

public record SSOLoginCallbackResult(SSOLoginResponseDTO result);

public class SSOLoginCallbackHandler : ICommandHandler<SSOLoginCallbackCommand, SSOLoginCallbackResult>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _config;
	private readonly ILogger<SSOLoginCallbackHandler> _logger;
	private readonly string _signinScheme;
	public SSOLoginCallbackHandler(
		IHttpContextAccessor httpContextAccessor,
		IConfiguration config,
		ILogger<SSOLoginCallbackHandler> logger)
	{
		_httpContextAccessor = httpContextAccessor;
		_config = config;
		_logger = logger;
		_signinScheme = config.GetValue<string>("SSOMetadata:SigninScheme")!;
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

		// Sign in with the NEW principal that has the correct authentication type
		await httpContext!.SignInAsync(
			_signinScheme,
			cookiePrincipal,
			new AuthenticationProperties
			{
				IsPersistent = true,
				ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
				AllowRefresh = true
			});

		return new SSOLoginCallbackResult(new SSOLoginResponseDTO(nameIdentifier!, name!, true));
	}
}
