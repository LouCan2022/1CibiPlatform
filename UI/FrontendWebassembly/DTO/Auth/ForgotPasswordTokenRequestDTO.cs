namespace FrontendWebassembly.DTO.Auth;

public record ForgotPasswordTokenRequestDTO(Guid userId, string tokenHash);
