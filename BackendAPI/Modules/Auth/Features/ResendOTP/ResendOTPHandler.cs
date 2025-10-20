namespace Auth.Features.ResendOTP;

public record ResendOTPCommand(OtpVerificationRequestDTO OtpVerificationRequestDTO) : ICommand<ResendOTPResult>;

public record ResendOTPResult(bool IsSuccess);


public class ResendOTPValidator : AbstractValidator<ResendOTPCommand>
{
	public ResendOTPValidator()
	{
		RuleFor(x => x.OtpVerificationRequestDTO.userId)
			.NotEmpty().WithMessage("UserId is required.");
		RuleFor(x => x.OtpVerificationRequestDTO.email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.");
	}
}


public class ResendOTPHandler : ICommandHandler<ResendOTPCommand, ResendOTPResult>
{
	private readonly IRegisterService _registerService;

	public ResendOTPHandler(IRegisterService registerService)
	{
		this._registerService = registerService;
	}

	public async Task<ResendOTPResult> Handle(
		ResendOTPCommand request,
		CancellationToken cancellationToken)
	{
		var isSent = await _registerService.ManualResendOtpCodeAsync(
			request.OtpVerificationRequestDTO.userId,
			request.OtpVerificationRequestDTO.email);

		return new ResendOTPResult(isSent);
	}
}
