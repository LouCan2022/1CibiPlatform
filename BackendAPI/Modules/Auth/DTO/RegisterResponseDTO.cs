namespace Auth.DTO;


public record RegisterResponseDTO(
	Guid Id,
	string Username,
	string Email,
	string PasswordHash,
	string FirstName,
	string LastName,
	string? MiddleName);