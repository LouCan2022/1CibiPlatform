namespace Auth.DTO;

public class UserDTO
{
	public Guid UserId { get; set; }
	public string? Email { get; set; } 
	public string? FirstName { get; set; } 
	public string? MiddleName { get; set; }
	public string? LastName { get; set; } 
	public bool IsApproved { get; set; }
}
