using FrontendWebassembly.DTO.Auth;

namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IAuthService
{
	Task<AuthResponseDTO> Login(LoginCred cred);

	Task<bool> IsAuthenticated();

	Task<string> GetUserInfoIfAuthenticated();

	Task<bool> Logout();

}
