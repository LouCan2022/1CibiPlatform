namespace FrontendWebassembly.DTO.Auth;

public record UpdatePasswordRequestDTO(
	Guid userId,
	string hashToken,
	string newPassword);
