namespace PhilSys.Data.Entities;

public class PhilSysTransaction
{
	[Key]
	public Guid Tid { get; set; }
	public int MyProperty { get; set; }
	public string? FirstName { get; set; }
	public string? MiddleName { get; set; }
	public string? LastName { get; set; }
	public string? Suffix { get; set; }
	public string? BirthDate { get; set; }
	public string? PCN { get; set; }
	public string? FaceLivenessSessionId { get; set; }
	public bool IsTransacted { get; set; } = false;
	public DateTime CreatedAt { get; set; }
	public DateTime? TransactedAt { get; set; }
}
