namespace Auth.Data.Repository;

public interface IAuthRepository
{

	Task<LoginDTO> GetUserDataAsync(LoginWebCred cred);

	Task<bool> SaveRefreshTokenAsync(Guid userId, string hashToken, DateTime expiryDate);

	Task<UserDataDTO> GetNewUserDataAsync(Guid userId);

	Task<bool> UpdateRevokeReasonAsync(string refreshToken, string reason);

}
