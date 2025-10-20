namespace FrontendWebassembly.DTO.PhilSys;

public class TransactionStatusResponse
{
	public bool Exists { get; set; }
	public bool IsTransacted { get; set; }
	public DateTime? ExpiresAt { get; set; }
}
