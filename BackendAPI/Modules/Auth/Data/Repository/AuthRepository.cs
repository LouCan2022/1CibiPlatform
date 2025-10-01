using Mapster;

namespace Auth.Data.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly AuthApplicationDbContext _dbcontext;

    public AuthRepository(AuthApplicationDbContext dbcontext)
    {
        this._dbcontext = dbcontext;
    }

    public async Task<LoginDTO> LoginAsync(LoginCred cred)
    {
        var userData = await _dbcontext.AuthUsers
            .Where(u => u.Username == cred.Username)
            .FirstOrDefaultAsync();

        var userDTO = userData.Adapt<LoginDTO>();

        return userDTO!;
    }
}
