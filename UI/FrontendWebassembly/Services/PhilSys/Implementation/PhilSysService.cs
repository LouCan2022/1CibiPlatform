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

		var response = await _httpClient.PostAsJsonAsync("philsys/idv/updatefacelivenesssession", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Update Successfully");
			return null!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<UpdateFaceLivenessSessionResponseDTO>();
		Console.WriteLine("✅ Update Successfully");

		return successContent!;
	}

	public async Task<TransactionStatusResponse> GetTransactionStatusAsync(Guid Tid)
	{
		var request = new { Tid };
		var response = await _httpClient.PostAsJsonAsync("/philsys/idv/validate/liveness", request);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Get the Status");
			return new TransactionStatusResponse
			{
				Exists = false
			};
		}
		
		var successContent = await response.Content.ReadFromJsonAsync<TransactionStatusResponse>();

		if (successContent!.ExpiresAt < DateTime.UtcNow)
		{;
			successContent!.isExpired = true;
		}
		
		
		Console.WriteLine("✅ Update Successfully");
		return successContent!;

	}

	public async Task<bool> DeleteTransactionAsync(Guid Tid)
	{
		var request = new { Tid };
		var response = await _httpClient.PostAsJsonAsync("/philsys/deletetransaction", request);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Update Successfully");
			return false!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Delete Successfully");
		return successContent!;
	}
}
