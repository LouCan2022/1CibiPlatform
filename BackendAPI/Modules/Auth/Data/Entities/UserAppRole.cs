namespace Auth.Data.Entities;

public class UserAppRole
{
    public int UserId { get; set; }
    public int AppId { get; set; }
    public int RoleId { get; set; }
    public int AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}