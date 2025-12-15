namespace FrontendWebassembly.Services.CNX.Implementation;

public class CandidateService : ICandidateService
{
	private readonly HttpClient _httpClient;

	public CandidateService(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient("API");
	}
	public async Task<PaginatedCNX> GetCandidates(string gmail, string page, CancellationToken ct = default)
	{
		var query = $"cnx/gettalkpushcandidate?request={gmail}&page={page}";
		var response = await _httpClient.GetFromJsonAsync<PaginatedCNX>(query, ct);

		return response!;
	}
}
