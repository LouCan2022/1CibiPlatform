namespace FrontendWebassembly.Services.PhilSys.Implementation;

public class PhilSysService : IPhilSysService
{
	private readonly HttpClient _httpClient;

	public PhilSysService(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}
	public async Task<Guid?> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSession)
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
			return Tid;
		}

		var successContent = await response.Content.ReadFromJsonAsync<UpdateFaceLivenessSessionResponseDTO>();
		Console.WriteLine("✅ Update Successfully");

		return successContent!.Tid;
	}
}
