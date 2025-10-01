namespace Auth.DTO;

public record LoginDTO(
    Guid Id,
    string Username,
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName,
    string? MiddleName,
    bool IsActive,
    DateTimeOffset CreatedAt);


public record LoginCred(
    string Username,
    string Password);