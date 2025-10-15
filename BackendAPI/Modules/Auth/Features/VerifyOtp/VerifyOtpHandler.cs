namespace Auth.Features.VerifyOtp;

public record VerifyOtpCommand(OtpRequestDTO otpRequest) : ICommand<VerifyOtpResult>;

public record VerifyOtpResult(bool IsVerified);


public class VerifyOtpHandler : ICommandHandler<VerifyOtpCommand, VerifyOtpResult>
{
	private readonly IRegisterService _registerService;
	private readonly ILogger<VerifyOtpHandler> _logger;

	public VerifyOtpHandler(
		IRegisterService registerService,
		ILogger<VerifyOtpHandler> logger)
	{
		this._registerService = registerService;
		this._logger = logger;
	}

	public async Task<VerifyOtpResult> Handle(
		VerifyOtpCommand request,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling VerifyOtpCommand for email: {Email}", request.otpRequest.Email);

		var result = await _registerService
			.VerifyOtpAsync(request.otpRequest.Email, request.otpRequest.Otp);

		return new VerifyOtpResult(result);
	}
}
