namespace FrontendWebassembly.Services.PhilSys.Implementation;

public class PhilSysService : IPhilSysService
{
	private readonly HttpClient _httpClient;

	public PhilSysService(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient("API");
	}

	public async Task<UpdateFaceLivenessSessionResponseDTO> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSession)
	{
		var payload = new
		{
			Tid,
			FaceLivenessSessionId = FaceLivenessSession
		};

		var  response = await _httpClient.PostAsJsonAsync("philsys/idv/updatefacelivenesssession", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Update Successfully");
			return null!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<UpdateFaceLivenessSessionResponseDTO>();
		Console.WriteLine("✅ Update Successfully");

		return successContent!;
	}

	public async Task<TransactionStatusResponse> GetTransactionStatus(Guid Tid)
	{
		var response = await _httpClient.PostAsJsonAsync("idv/validate/liveness", Tid);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Get the Status");
			return null!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<TransactionStatusResponse>();
		Console.WriteLine("✅ Update Successfully");

		return successContent!;

	}
}
