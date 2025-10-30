namespace SSO.Features.Logout;

public record SSOLogoutCommand : ICommand;

public class SSOLogoutHandler : ICommandHandler<SSOLogoutCommand>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<SSOLogoutHandler> _logger;
	private readonly IConfiguration _configuration;
	private readonly string _signinScheme;

	public SSOLogoutHandler(
	 IHttpContextAccessor httpContextAccessor,
	 ILogger<SSOLogoutHandler> logger,
	 IConfiguration configuration)
	{
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
		_signinScheme = configuration.GetValue<string>("SSOMetadata:SigninScheme")!;
	}

	public async Task<Unit> Handle(SSOLogoutCommand request, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		_logger.LogInformation("User logging out");

		await httpContext!.SignOutAsync(_signinScheme);

		return Unit.Value;
	}
}
