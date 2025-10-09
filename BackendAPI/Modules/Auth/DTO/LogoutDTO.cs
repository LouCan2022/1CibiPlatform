namespace Auth.DTO;

public record LogoutDTO(
	Guid userId,
	string revokeReason
	);
