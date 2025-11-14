namespace Auth.DTO;

public record AddApplicationDTO
{
<<<<<<< Updated upstream
=======
	public int AppId { get; set; }
>>>>>>> Stashed changes
	public string AppName { get; set; } = null!;
	public string AppCode { get; set; } = null!;
	public string? Description { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
