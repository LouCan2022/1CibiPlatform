using Auth.DTO;

namespace Auth.Data.Repository;

public class AuthRepository : IAuthRepository
{
    public Task<LoginDTO> LoginAsync(LoginCred cred)
    {
        throw new NotImplementedException();
    }
}
