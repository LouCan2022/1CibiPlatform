namespace SSO.Features.LoginCallback;

public record SSOLoginCallbackCommand(string ReturnUrl = "/") : ICommand<SSOLoginCallbackResult>;

public record SSOLoginCallbackResult(SSOLoginResponseDTO result);

public class SSOLoginCallbackHandler : ICommandHandler<SSOLoginCallbackCommand, SSOLoginCallbackResult>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _config;
	private readonly ILogger<SSOLoginCallbackHandler> _logger;

	public SSOLoginCallbackHandler(
		IHttpContextAccessor httpContextAccessor,
		IConfiguration config,
		ILogger<SSOLoginCallbackHandler> logger)
	{
		_httpContextAccessor = httpContextAccessor;
		_config = config;
		_logger = logger;
	}

	public async Task<SSOLoginCallbackResult> Handle(SSOLoginCallbackCommand request, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		// Authenticate using the temporary SAML cookie
		AuthenticateResult result = await httpContext!.AuthenticateAsync("AppExternalScheme");

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


		// Sign out from temporary SAML cookie
		await httpContext!.SignOutAsync("AppExternalScheme");


		return new SSOLoginCallbackResult(new SSOLoginResponseDTO(email!, name!, true));
	}
}
