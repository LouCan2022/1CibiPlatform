namespace SSO.Features.IsUserAuthenticated;

public record IsUserAuthenticatedCommand() : ICommand<IsUserAuthenticatedResult>;

public record IsUserAuthenticatedResult(bool IsAuthenticated);

public class IsUserAuthenticatedHandler : ICommandHandler<IsUserAuthenticatedCommand, IsUserAuthenticatedResult>
{
	private readonly ILogger<IsUserAuthenticatedHandler> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _configuration;
	private readonly string _signinScheme;

	public IsUserAuthenticatedHandler(
		ILogger<IsUserAuthenticatedHandler> logger,
		IHttpContextAccessor httpContextAccessor,
		IConfiguration configuration)
	{
		this._logger = logger;
		this._httpContextAccessor = httpContextAccessor;
		this._configuration = configuration;
		this._signinScheme = configuration.GetValue<string>("SSOMetadata:SigninScheme")!;
	}

	public async Task<IsUserAuthenticatedResult> Handle(IsUserAuthenticatedCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Checking if user is authenticated.");

		var httpContext = _httpContextAccessor.HttpContext;

		var authResult = await httpContext!.AuthenticateAsync(_signinScheme);

		if (authResult?.Succeeded == true && authResult.Principal?.Identity?.IsAuthenticated == true)
		{
			_logger.LogInformation("User is authenticated via SSO");
			return new IsUserAuthenticatedResult(true);
		}

		_logger.LogInformation("User is NOT authenticated");
		return new IsUserAuthenticatedResult(false);
	}
}
