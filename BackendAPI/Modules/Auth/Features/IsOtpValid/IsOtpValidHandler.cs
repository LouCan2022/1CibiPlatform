namespace Auth.Features.IsOtpValid;

public record IsOtpValidCommand(OtpVerificationRequestDTO OtpVerificationRequestDto) : ICommand<IsOtpValidResult>;

public record IsOtpValidResult(bool isOtpVerfied);

public class IsOtpValidCommandValidator : AbstractValidator<IsOtpValidCommand>
{
	public IsOtpValidCommandValidator()
	{

		RuleFor(x => x.OtpVerificationRequestDto.email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.");

		RuleFor(x => x.OtpVerificationRequestDto.userId)
				.NotEmpty().WithMessage("userId is required.");
	}
}


public class IsOtpValidHandler : ICommandHandler<IsOtpValidCommand, IsOtpValidResult>
{
	private readonly IRegisterService _registerService;
	private readonly ILogger<IsOtpValidHandler> _logger;

	public IsOtpValidHandler(
		IRegisterService registerService,
		ILogger<IsOtpValidHandler> logger)
	{
		this._registerService = registerService;
		this._logger = logger;
	}

	public async Task<IsOtpValidResult> Handle(IsOtpValidCommand request, CancellationToken cancellationToken)
	{

		_logger.LogInformation("Handling IsOtpValidCommand for UserId: {UserId}, Email: {Email}",
			request.OtpVerificationRequestDto.userId, request.OtpVerificationRequestDto.email);

		var isOtpValidResult = await _registerService.IsOtpSessionValidAsync(
			request.OtpVerificationRequestDto.userId,
			request.OtpVerificationRequestDto.email);

		if (!isOtpValidResult)
		{
			_logger.LogWarning("OTP is not valid");
			return new IsOtpValidResult(false);
		}

		return new IsOtpValidResult(isOtpValidResult);

	}
}
