
namespace Auth.Features.Register;

public record RegisterRequestCommand(RegisterRequestDTO register) : ICommand<RegisterResult>;

public record RegisterResult(OtpVerificationResponse otpVerificationResponse);

public class RegisterRequestCommandValidator : AbstractValidator<RegisterRequestCommand>
{
	public RegisterRequestCommandValidator()
	{
		RuleFor(x => x.register.Email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.");
		RuleFor(x => x.register.PasswordHash)
			.NotEmpty().WithMessage("Password is required.")
			.MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
			.MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
			.Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
			.Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
			.Matches(@"[0-9]").WithMessage("Password must contain at least one digit.")
			.Matches(@"[\W_]").WithMessage("Password must contain at least one special character.");
		RuleFor(x => x.register.FirstName)
			.NotEmpty().WithMessage("First name is required.")
			.MaximumLength(50).WithMessage("First name must not exceed 50 characters.");
		RuleFor(x => x.register.LastName)
			.NotEmpty().WithMessage("Last name is required.")
			.MaximumLength(50).WithMessage("Last name must not exceed 50 characters.");
		RuleFor(x => x.register.MiddleName)
			.MaximumLength(50).WithMessage("Middle name must not exceed 50 characters.")
			.When(x => !string.IsNullOrEmpty(x.register.MiddleName));
	}
}


public class RegisterHandler : ICommandHandler<RegisterRequestCommand, RegisterResult>
{
	private readonly IRegisterService _registerService;

	public RegisterHandler(IRegisterService registerService)
	{
		this._registerService = registerService;
	}


	public async Task<RegisterResult> Handle(
		RegisterRequestCommand request,
		CancellationToken cancellationToken)
	{

		var otpVerfication = await this._registerService.RegisterAsync(request.register);

		return new RegisterResult(otpVerfication);

	}
}
