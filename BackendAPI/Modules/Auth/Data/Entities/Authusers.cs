namespace Auth.Data.Entities;

public class Authusers
{
	public Guid Id { get; set; }
	public string Email { get; set; } = string.Empty;
	public string PasswordHash { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string? MiddleName { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; }
}
