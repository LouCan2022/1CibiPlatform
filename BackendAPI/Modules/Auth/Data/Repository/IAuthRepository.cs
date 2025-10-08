namespace Auth.Data.Repository;

public interface IAuthRepository
{

	Task<LoginDTO> GetUserDataAsync(LoginCred cred);

	Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate);

	Task<LoginDTO> GetNewUserDataAsync(string refreshToken);

	Task<bool> UpdateRevokeReasonAsync(string refreshToken, string reason);

}
