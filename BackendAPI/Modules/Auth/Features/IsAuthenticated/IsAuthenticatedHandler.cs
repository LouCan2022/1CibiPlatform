namespace Auth.Features.IsAuthenticated;

public record IsAuthenticatedCommand() : ICommand<IsAuthenticatedResult>;

public record IsAuthenticatedResult(bool IsAuthenticated);

public class IsAuthenticatedHandler : ICommandHandler<IsAuthenticatedCommand, IsAuthenticatedResult>
{
	private readonly ILoginService _loginService;

	public IsAuthenticatedHandler(ILoginService loginService)
	{
		this._loginService = loginService;
	}
	public async Task<IsAuthenticatedResult> Handle(
		IsAuthenticatedCommand request,
		CancellationToken cancellationToken)
	{
		var isAuthenticated = await _loginService.IsAuthenticated();

		return new IsAuthenticatedResult(isAuthenticated);
	}
}
