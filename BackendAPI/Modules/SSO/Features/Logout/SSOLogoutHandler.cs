namespace SSO.Features.Logout;

public record SSOLogoutCommand : ICommand;

public class SSOLogoutHandler : ICommandHandler<SSOLogoutCommand>
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILogger<SSOLogoutHandler> _logger;

	public SSOLogoutHandler(
	 IHttpContextAccessor httpContextAccessor,
	 ILogger<SSOLogoutHandler> logger)
	{
		_httpContextAccessor = httpContextAccessor;
		_logger = logger;
	}

	public async Task<Unit> Handle(SSOLogoutCommand request, CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		_logger.LogInformation("User logging out");

		await httpContext!.SignOutAsync(Saml2Defaults.Scheme);

		return Unit.Value;
	}
}
