namespace PhilSys.DTO;

public record CredentialResponseDTO(
	string access_token,
	string token_type,
	string expires_at
);

public record PhilSysTokenResponse(
	CredentialResponseDTO data
);

