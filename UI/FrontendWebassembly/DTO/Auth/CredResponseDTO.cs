namespace FrontendWebassembly.DTO.Auth;

public record CredResponseDTO(Guid UserId,
	string AccessToken,
	string TokenType,
	int ExpiresIn,
	string UserName,
	string Issued,
	string Expires);


public record IsAuthenticatedDTO(bool isAuthenticated);


public record LogoutResponseDTO(bool isLoggedOut);

