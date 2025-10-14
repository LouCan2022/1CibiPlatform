namespace Auth.Services;

public interface IRegisterService
{
	Task<OtpVerificationDTO> RegisterAsync(RegisterRequestDTO registerRequestDTO);
}
