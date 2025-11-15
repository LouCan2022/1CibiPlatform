namespace Auth.DTO;

public class EditApplicationDTO
{
	public int AppId { get; set; }
	public string? AppName { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
}
