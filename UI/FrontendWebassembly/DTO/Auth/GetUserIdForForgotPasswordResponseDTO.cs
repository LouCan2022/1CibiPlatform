namespace FrontendWebassembly.DTO.Auth;

public record GetUserIdForForgotPasswordResponseDTO(Guid UserId, string errorMessage);
