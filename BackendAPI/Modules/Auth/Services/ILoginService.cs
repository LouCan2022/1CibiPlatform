namespace Auth.Services;

public interface ILoginService
{
	Task<LoginResponseDTO> LoginAsync(LoginCred cred);

	Task<LoginResponseWebDTO> LoginWebAsync(LoginWebCred cred);
}
