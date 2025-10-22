namespace SSO.Features.Login;

public record SSOLoginCommand(string ReturnUrl = "/") : ICommand;


public class SSOLoginHandler : ICommandHandler<SSOLoginCommand>
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public SSOLoginHandler(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<Unit> Handle(
		SSOLoginCommand request,
		CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;
		var props = new AuthenticationProperties
		{
			RedirectUri = $"/sso/login/callback?returnUrl={request.ReturnUrl}",
			IsPersistent = true
		};

		await httpContext!.ChallengeAsync(Saml2Defaults.Scheme, props);

		return Unit.Value;
	}

}