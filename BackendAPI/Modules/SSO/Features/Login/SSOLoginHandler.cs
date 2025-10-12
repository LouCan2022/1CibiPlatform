namespace SSO.Features.Login;

public record SSOLoginCommand(string ReturnUrl = "/") : ICommand;


public class SSOLoginHandler : ICommandHandler<SSOLoginCommand>
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public SSOLoginHandler(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public Task<Unit> Handle(
		SSOLoginCommand request,
		CancellationToken cancellationToken)
	{
		var httpContext = _httpContextAccessor.HttpContext;

		var props = new AuthenticationProperties
		{
			RedirectUri = $"/saml/callback?returnUrl={request.ReturnUrl}",
			IsPersistent = true
		};

		var result = Results.Challenge(props, new[] { Saml2Defaults.Scheme });

		return Task.FromResult(Unit.Value);
	}

}