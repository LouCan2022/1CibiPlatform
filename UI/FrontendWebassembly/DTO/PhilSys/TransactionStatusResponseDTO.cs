namespace FrontendWebassembly.DTO.PhilSys;

public record TransactionStatusResponseDTO
{
	public bool Exists { get; set; }
	public string? WebHookUrl { get; set; }
	public bool IsTransacted { get; set; }
	public bool isExpired { get; set; } = false;
	public DateTime ExpiresAt { get; set; }
}
