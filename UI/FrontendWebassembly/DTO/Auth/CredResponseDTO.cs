namespace FrontendWebassembly.DTO.Auth;

public record CredResponseDTO(Guid UserId,
	string AccessToken,
	string TokenType,
	int ExpiresIn,
	string UserName,
	List<int> Appid,
	List<List<int>> SubMenuid,
	List<int> RoleId,
	string Issued,
	string Expires);


public record IsAuthenticatedDTO(bool isAuthenticated);


public record LogoutResponseDTO(bool isLoggedOut);

