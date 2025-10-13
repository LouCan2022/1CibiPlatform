namespace Auth.DTO;

public record RegisterRequestDTO(
	Guid Id,
	string Username,
	string Email,
	string PasswordHash,
	string FirstName,
	string LastName,
	string? MiddleName,
	bool IsActive,
	DateTimeOffset CreatedAt);