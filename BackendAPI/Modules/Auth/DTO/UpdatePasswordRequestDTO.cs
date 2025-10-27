namespace Auth.DTO;

public record UpdatePasswordRequestDTO(
	Guid userId,
	string hashToken,
	string newPassword);

