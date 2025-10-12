namespace SSO.DTO;

public record SSOLoginResponseDTO
(
	string Email,
	string Name,
	bool Success
);