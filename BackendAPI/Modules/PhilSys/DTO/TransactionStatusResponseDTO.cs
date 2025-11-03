namespace PhilSys.DTO;
public record TransactionStatusResponseDTO
{
	public bool Exists { get; set; } = true;
	public string? WebHookURl { get; set; }
	public bool IsTransacted { get; set; }
	public bool isExpired { get; set; } = false;
	public DateTime ExpiresAt { get; set; }
}
