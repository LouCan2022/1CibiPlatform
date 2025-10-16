namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IAuthService
{
	Task<AuthResponseDTO> Login(LoginCred cred);

	Task<bool> IsAuthenticated();

	Task<string> GetUserInfoIfAuthenticated();


	Task<RegisterResponseDTO> Register(RegisterRequestDTO registerRequestDTO);


	Task<OtpSessionResponseDTO> IsOtpSessionValid(OtpSessionRequestDTO otpRequestDTO);

	Task<OtpSessionResponseDTO> OtpVerification(OtpVerificationRequestDTO otpVerificationRequestDTO);

	Task<bool> Logout();

}
