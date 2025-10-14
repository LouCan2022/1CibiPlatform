
namespace Auth.Features.Register;

public record RegisterRequestCommand(RegisterRequestDTO register) : ICommand<RegisterResult>;

public record RegisterResult(OtpVerificationResponse otpVerificationResponse);


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
