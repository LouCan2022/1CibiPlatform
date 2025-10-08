namespace Auth.Services;

public interface IRefreshTokenService
{
	(string, string) GenerateRefreshToken();

	bool ValidateHashToken(string providedToken, string storedHash);

	Task<string> RevokeToken();

	Task<LoginResponseWebDTO> GetNewAccessToken(string refreshToken);
}
