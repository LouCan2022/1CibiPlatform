namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IRefreshTokenService
{
	Task<AuthResponseDTO> GetNewAccessAndRefreshToken(Guid userId);

	Task<bool> Logout();
}
