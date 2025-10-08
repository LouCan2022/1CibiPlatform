namespace Auth.Features.Login;

public record LoginCommand(LoginCred LoginCred) : ICommand<LoginResult>;

public record LoginResult(LoginResponseDTO loginResponseDTO);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
	public LoginCommandValidator()
	{
		RuleFor(x => x.LoginCred.Username)
			.NotEmpty().WithMessage("Username is required.")
			.MaximumLength(50).WithMessage("Username must not exceed 50 characters.");
		RuleFor(x => x.LoginCred.Password)
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
		var loginResponse = await this._loginService.LoginAsync(request.LoginCred);
		return new LoginResult(loginResponse);
	}

}



