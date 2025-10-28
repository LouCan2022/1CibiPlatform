namespace FrontendWebassembly.Services.SSO.Interfaces;

public interface ISSOService
{
	Task<bool> IsUserAuthenticatedAsync();

	Task<bool> LogoutAsync();

}
