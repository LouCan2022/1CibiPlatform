namespace Auth.Features.Logout;

public record LogoutCommand(LogoutDTO logoutDTO) : ICommand<LogoutResult>;

public record LogoutResult(bool IsLoggedOut);

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
	public LogoutCommandValidator()
	{
		RuleFor(x => x.logoutDTO.userId)
			.NotEmpty().WithMessage("UserId is required.");

		RuleFor(x => x.logoutDTO.revokeReason)
			.NotEmpty().WithMessage("Revoke Reason is required.");
	}
}

public class LogoutHandler : ICommandHandler<LogoutCommand, LogoutResult>
{
	private readonly ILoginService _loginService;
	public LogoutHandler(ILoginService loginService)
	{
		this._loginService = loginService;
	}
	public async Task<LogoutResult> Handle(
		LogoutCommand request,
		CancellationToken cancellationToken)
	{
		var isLoggedOut = await this._loginService.LogoutAsync(
			request.logoutDTO.userId,
			request.logoutDTO.revokeReason);

		return new LogoutResult(isLoggedOut);
	}
}

