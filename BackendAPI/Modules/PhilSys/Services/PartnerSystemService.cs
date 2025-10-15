namespace PhilSys.Services;

public class PartnerSystemService
{
	private readonly HttpClient _httpClientFactory;
	private readonly ILogger<PartnerSystemService> _logger;

	public PartnerSystemService(
		IHttpClientFactory httpClientFactory,
		ILogger<PartnerSystemService> logger)
	{
		_httpClientFactory = httpClientFactory.CreateClient("PhilSys");
		_logger = logger;
	}
	public async Task<PartnerSystemResponseDTO> PartnerSystemQueryAsync(PartnerSystemRequestDTO PartnerSystemRequestDTO)
	{
		return null!;
	}
}
