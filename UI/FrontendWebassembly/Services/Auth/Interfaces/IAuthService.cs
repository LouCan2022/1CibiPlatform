namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IAuthService
{
	Task<string> Login(LoginCred cred);

	Task<string> GetUserInfoIfAuthenticated();

}
