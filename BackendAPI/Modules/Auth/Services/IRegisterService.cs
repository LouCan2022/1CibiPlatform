namespace Auth.Services;

public interface IRegisterService
{
	Task<OtpVerificationResponse> RegisterAsync(RegisterRequestDTO registerRequestDTO);
}
