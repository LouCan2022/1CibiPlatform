namespace Auth.Features.LoginWeb;

public class LoginWebHandler
{
	public record LoginWebCommand(LoginCred LoginCred) : ICommand<LoginWebResult>;

	public record LoginWebResult(LoginResponseWebDTO loginResponseWebDTO);

	public class LoginCommandValidator : AbstractValidator<LoginWebCommand>
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

	public class LoginWHandler : ICommandHandler<LoginWebCommand, LoginWebResult>
	{
		private readonly ILoginService _loginService;

		public LoginWHandler(ILoginService loginService)
		{
			this._loginService = loginService;
		}

		public async Task<LoginWebResult> Handle(
			LoginWebCommand request,
			CancellationToken cancellationToken)
		{

			var loginResponse = await this._loginService.LoginWebAsync(request.LoginCred);

			return new LoginWebResult(loginResponse);
		}
	}
}
