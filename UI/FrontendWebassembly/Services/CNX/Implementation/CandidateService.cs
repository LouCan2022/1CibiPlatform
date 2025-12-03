namespace FrontendWebassembly.Services.CNX.Implementation;

public class CandidateService : ICandidateService
{
	private readonly HttpClient _httpClient;

	public CandidateService(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient("API");
	}
	public async Task<List<CandidateResponseDTO>> GetCandidates(string gmail, CancellationToken ct = default)
	{
		var query = $"cnx/gettalkpushcandidate?request={gmail}";
		var response = await _httpClient.GetFromJsonAsync<List<CandidateResponseDTO>>(query, ct);

		return response!;
	}
}
