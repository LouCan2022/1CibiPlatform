﻿namespace FrontendWebassembly.Services.PhilSys.Implementation;

public class PhilSysService : IPhilSysService
{
	private readonly HttpClient _httpClient;

	public PhilSysService(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient("API");
	}

	public async Task<UpdateFaceLivenessSessionResponseDTO> UpdateFaceLivenessSessionAsync(string HashToken, string FaceLivenessSession)
	{
		var payload = new
		{
			HashToken,
			FaceLivenessSessionId = FaceLivenessSession
		};

		var response = await _httpClient.PostAsJsonAsync("philsys/idv/updatefacelivenesssession", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Update Successfully");
			throw new Exception("Error in Updating Face Liveness Session");
		}

		var successContent = await response.Content.ReadFromJsonAsync<UpdateFaceLivenessSessionResponseDTO>();

		Console.WriteLine("✅ Update Successfully");

		return successContent!;
	}

	public async Task<TransactionStatusResponse> GetTransactionStatusAsync(string HashToken)
	{
		var request = new { HashToken };
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
		
		Console.WriteLine("✅ Retrieve the Status Successfully");
		 
		return successContent!;

	}

	public async Task<bool> DeleteTransactionAsync(string HashToken)
	{
		var request = new { HashToken };
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

	public async Task<string> PostBasicInformationOrPCN(string inquiry_type, IdentityData identity_data)
	{
		if (DateTime.TryParse(identity_data.birth_date, out var parsedDate))
		{
			identity_data.birth_date = parsedDate.ToString("yyyy-MM-dd");
		}
		if (inquiry_type == "name_dob")
		{
			var requestInfo = new { callback_url = "/", inquiry_type = "name_dob", identity_data };
			var responseInfo = await _httpClient.PostAsJsonAsync("philsys/idv", requestInfo);
			if (!responseInfo.IsSuccessStatusCode)
			{
				Console.WriteLine("❌ Did not Update Successfully");
				return "";
			}

			var successContentInfo = await responseInfo.Content.ReadFromJsonAsync<PostBasicInformationOrPCNResponseDTO>();
			return successContentInfo!.liveness_link!;
		}

		var requestPcn = new { callback_url = "/", inquiry_type = "pcn", identity_data };
		var responsePCn = await _httpClient.PostAsJsonAsync("philsys/idv", requestPcn);
		if (!responsePCn.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Update Successfully");
			return "";
		}

		var successContentPcn = await responsePCn.Content.ReadFromJsonAsync<PostBasicInformationOrPCNResponseDTO>();
		return successContentPcn!.liveness_link!;

	}
}
