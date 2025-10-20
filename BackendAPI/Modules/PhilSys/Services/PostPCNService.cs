namespace PhilSys.Services;
public class PostPCNService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<PostPCNService> _logger;

	public PostPCNService(
		IHttpClientFactory httpClientFactory,
		ILogger<PostPCNService> logger)
	{
		_httpClient = httpClientFactory.CreateClient("PhilSys");
		_logger = logger;
	}

	public async Task<BasicInformationOrPCNResponseDTO> PostPCNAsync(
		string value,
		string face_liveness_session_id,
		string bearer_token,
		CancellationToken ct = default
		)
	{
		var endpoint = "query/qr";

		var body = new
		{
			value,
			face_liveness_session_id
		};

		_logger.LogInformation("Sending PCN request to PhilSys endpoint: {Endpoint}", endpoint);

		_httpClient.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearer_token);
	
		var response = await _httpClient.PostAsJsonAsync(endpoint, body, ct);

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("PCN request failed: {Status}", response.StatusCode);

			var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(ct);

			return new BasicInformationOrPCNResponseDTO(
				code: "",
				token: "",
				reference: "",
				face_url: "",
				full_name: "",
				first_name: "",
				middle_name: "",
				last_name: "",
				suffix: "",
				gender: "",
				marital_status: "",
				blood_type: "",
				email: "",
				mobile_number: "",
				birth_date: "",
				full_address: "",
				address_line_1: "",
				address_line_2: "",
				barangay: "",
				municipality: "",
				province: "",
				country: "",
				postal_code: "",
				present_full_address: "",
				present_address_line_1: "",
				present_address_line_2: "",
				present_barangay: "",
				present_municipality: "",
				present_province: "",
				present_country: "",
				present_postal_code: "",
				residency_status: "",
				place_of_birth: "",
				pob_municipality: "",
				pob_province: "",
				pob_country: "",
				error: errorResponse?.error,
				message: errorResponse?.message,
				error_description: errorResponse?.error_description
			);
		}

		var responseBody = await response.Content.ReadFromJsonAsync<PostBasicInformationOrPCNResponse>(ct);

		if (responseBody is null || responseBody.data is null)
		{
			throw new InvalidOperationException("Response body or data is null.");
		}

		var returnData = responseBody.data;

		return ReturnData(returnData);

	}

	private static BasicInformationOrPCNResponseDTO ReturnData(BasicInformationOrPCNResponseDTO BasicInformationOrPCNResponseDTO)
	{
		return new BasicInformationOrPCNResponseDTO(
			BasicInformationOrPCNResponseDTO.code,
			BasicInformationOrPCNResponseDTO.token,
			BasicInformationOrPCNResponseDTO.reference,
			BasicInformationOrPCNResponseDTO.face_url,
			BasicInformationOrPCNResponseDTO.full_name,
			BasicInformationOrPCNResponseDTO.first_name,
			BasicInformationOrPCNResponseDTO.middle_name,
			BasicInformationOrPCNResponseDTO.last_name,
			BasicInformationOrPCNResponseDTO.suffix,
			BasicInformationOrPCNResponseDTO.gender,
			BasicInformationOrPCNResponseDTO.marital_status,
			BasicInformationOrPCNResponseDTO.blood_type,
			BasicInformationOrPCNResponseDTO.email,
			BasicInformationOrPCNResponseDTO.mobile_number,
			BasicInformationOrPCNResponseDTO.birth_date,
			BasicInformationOrPCNResponseDTO.full_address,
			BasicInformationOrPCNResponseDTO.address_line_1,
			BasicInformationOrPCNResponseDTO.address_line_2,
			BasicInformationOrPCNResponseDTO.barangay,
			BasicInformationOrPCNResponseDTO.municipality,
			BasicInformationOrPCNResponseDTO.province,
			BasicInformationOrPCNResponseDTO.country,
			BasicInformationOrPCNResponseDTO.postal_code,
			BasicInformationOrPCNResponseDTO.present_full_address,
			BasicInformationOrPCNResponseDTO.present_address_line_1,
			BasicInformationOrPCNResponseDTO.present_address_line_2,
			BasicInformationOrPCNResponseDTO.present_barangay,
			BasicInformationOrPCNResponseDTO.present_municipality,
			BasicInformationOrPCNResponseDTO.present_province,
			BasicInformationOrPCNResponseDTO.present_country,
			BasicInformationOrPCNResponseDTO.present_postal_code,
			BasicInformationOrPCNResponseDTO.residency_status,
			BasicInformationOrPCNResponseDTO.place_of_birth,
			BasicInformationOrPCNResponseDTO.pob_municipality,
			BasicInformationOrPCNResponseDTO.pob_province,
			BasicInformationOrPCNResponseDTO.pob_country,
			BasicInformationOrPCNResponseDTO.error,
			BasicInformationOrPCNResponseDTO.message,
			BasicInformationOrPCNResponseDTO.error_description
			);
	}


	protected virtual async Task<HttpResponseMessage> SendRequestAsync(
		string endpoint,
		object body,
		CancellationToken ct)
	{
		return await _httpClient.PostAsJsonAsync(endpoint, body, ct);
	}
}