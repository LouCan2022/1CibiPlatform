namespace Auth.Features.Login;

public record LoginCommand(string username, string password) : ICommand<LoginResult>;

public record LoginResult(LoginResponseDTO loginResponseDTO);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(x => x.username)
			.NotEmpty().WithMessage("Username is required.")
			.MaximumLength(50).WithMessage("Username must not exceed 50 characters.");

		RuleFor(x => x.password)
			.NotEmpty().WithMessage("Password is required.");
	}
}

public class LoginHandler : ICommandHandler<LoginCommand, LoginResult>
{

	private readonly ILoginService _loginService;

	public LoginHandler(ILoginService loginService)
	{
		this._loginService = loginService;
	}


	public async Task<LoginResult> Handle(
		LoginCommand request,
		CancellationToken cancellationToken)
	{
		var loginResponse = await this._loginService.LoginAsync(request.username, request.password);
		return new LoginResult(loginResponse);
	}

}



