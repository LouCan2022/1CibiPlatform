namespace FrontEnd.Services.Auth.IRepository;

public interface IAuthRepository
{
    Task<string> Login(LoginCred cred);

    Task<bool> SetToken(string token);

    Task<bool> IsTokenExist();

    Task<bool> RemoveToken();
}
