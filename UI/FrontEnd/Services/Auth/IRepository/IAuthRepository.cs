namespace FrontEnd.Services.Auth.IRepository;

public interface IAuthRepository
{
	Task<string> Login(LoginCred cred);

	Task<string> ReadToken();

}
