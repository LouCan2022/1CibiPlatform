namespace Auth.Data.Repository;

public interface IAuthRepository
{

	Task<LoginDTO> LoginAsync(LoginCred cred);

	Task<bool> SaveRefreshToken(Guid userId, string refreshToken, DateTime expiryDate);

}
