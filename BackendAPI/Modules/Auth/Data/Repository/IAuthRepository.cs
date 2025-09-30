using Auth.DTO;

namespace Auth.Data.Repository;

public interface IAuthRepository
{

    Task<LoginDTO> LoginAsync(LoginCred cred);

}
