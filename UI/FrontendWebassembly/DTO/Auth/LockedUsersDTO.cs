namespace FrontendWebassembly.DTO.Auth;

public record LockedUsersDTO
{
	public Guid UserId { get; set; }
	public string? Email { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public record LockedUsersResponseDTO
{
	public PaginatedResult<LockedUsersDTO>? lockedusers { get; set; }
}
