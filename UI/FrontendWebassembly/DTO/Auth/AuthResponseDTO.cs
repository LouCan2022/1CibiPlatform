namespace FrontendWebassembly.DTO.Auth;

public record AuthResponseDTO
	(Guid userId,
	 string token,
	 string detail,
	 string errorMessage);