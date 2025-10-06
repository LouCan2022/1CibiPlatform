
namespace PhilSys.DTO;

public record CredentialResponseDTO(
	string AccessToken,
	string TokenType,
	string ExpiresAt
);