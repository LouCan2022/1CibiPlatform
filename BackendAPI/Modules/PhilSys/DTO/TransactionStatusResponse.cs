namespace PhilSys.DTO;
public class TransactionStatusResponse
{
	public bool Exists { get; set; } = true;
	public bool IsTransacted { get; set; }
	public bool isExpired { get; set; } = false;
	public DateTime ExpiresAt { get; set; }
}
