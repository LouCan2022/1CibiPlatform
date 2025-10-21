namespace FrontendWebassembly.DTO.PhilSys;

public class TransactionStatusResponse
{
	public bool Exists { get; set; }
	public bool IsTransacted { get; set; }
	public bool isExpired { get; set; } = false;
	public DateTime ExpiresAt { get; set; }
}
