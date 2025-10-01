namespace Auth.DTO;

public record LoginDTO(
    Guid Id,
    string Username,
    string PasswordHash,
    string Email,
    string FirstName,
    string LastName,
    string? MiddleName,
    List<string> AppId,
    List<string> roleId
    );


public record LoginCred(
    string Username,
    string Password);