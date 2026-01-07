namespace SSO.Features.Logout;

public record SSOLogoutCommand : ICommand;

public class SSOLogoutHandler : ICommandHandler<SSOLogoutCommand>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<SSOLogoutHandler> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _signinScheme;
	private readonly string _userEmailCookieName;

	public SSOLogoutHandler(
	 IHttpContextAccessor httpContextAccessor,
	 ILogger<SSOLogoutHandler> logger,
	 IConfiguration configuration)
	{
		_configuration = configuration;
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
		_signinScheme = configuration.GetValue<string>("SSOMetadata:SigninScheme")!;
		_userEmailCookieName = configuration.GetValue<string>("SSOMetadata:UserEmailCookieName")!;
	}

	public async Task<Unit> Handle(SSOLogoutCommand request, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		_logger.LogInformation("User logging out");

		// remove http cookie only
		_httpContextAccessor.HttpContext!.Response.Cookies.Delete(_userEmailCookieName);

		await httpContext!.SignOutAsync(_signinScheme);

		return Unit.Value;
	}
}
