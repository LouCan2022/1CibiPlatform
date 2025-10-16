namespace Auth.DTO;

public record OtpVerificationRequestDTO(Guid userId, string email);
