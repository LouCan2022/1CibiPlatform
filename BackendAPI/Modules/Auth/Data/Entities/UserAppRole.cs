namespace Auth.Data.Entities;

public class AuthUserAppRole
{
    public Guid UserId { get; set; }
    public int AppId { get; set; }
    public int RoleId { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}