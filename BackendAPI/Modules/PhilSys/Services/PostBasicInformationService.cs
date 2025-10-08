namespace PhilSys.Services;
public class PostBasicInformationService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<PostBasicInformationService> _logger;

	public PostBasicInformationService(
		IHttpClientFactory httpClientFactory,
		ILogger<PostBasicInformationService> logger)
	{
		_httpClient = httpClientFactory.CreateClient("PhilSys");
		_logger = logger;
	}

	public async Task<BasicInformationResponseDTO> PostBasicInformationAsync(
		string first_name,
		string middle_name,
		string last_name,
		string suffix,
		DateTime birth_date,
		string face_liveness_session_id,
		string bearerToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL3dzLmV2ZXJpZnkuZ292LnBoIiwic3ViIjoiNDI1IiwianRpIjoiNjhlNjE1ZWU4YjA2MyIsImlhdCI6MTc1OTkwOTM1OC41Njk0NDgsIm5iZiI6MTc1OTkwOTM1OC41Njk0NDgsImV4cCI6MTc1OTkxMTE1OC41Njk0NDh9.VNeaJ_nsqncwEnYFF1dWq-HHmzbWsDp9Vi9dnE3mtpA", 
		CancellationToken ct = default
		)
	{
		var endpoint = "query";

		var body = new 
		{
			first_name = first_name,
			middle_name = middle_name,
			last_name = last_name,
			suffix = suffix,
			birth_date = birth_date.ToString("yyyy-MM-dd"),
			face_liveness_session_id = face_liveness_session_id
		};

		_logger.LogInformation("Sending basic information request to PhilSys endpoint: {Endpoint}", endpoint);

		_httpClient.DefaultRequestHeaders.Authorization =
			new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
		//var response = await SendRequestAsync(endpoint, body, ct);
		var response = await _httpClient.PostAsJsonAsync(endpoint, body, ct);

		var responseBody = await response.Content.ReadFromJsonAsync<PostBasicInformationResponse>(ct);
		_logger.LogInformation("Basic Information Response: {Response}", responseBody);

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("Basic Information request failed: {Status} - {Body}", response.StatusCode, responseBody);
			throw new HttpRequestException($"PhilSys token request failed: {response.StatusCode} - {responseBody}");
		}

		var tokenData = responseBody!.data;

		return new BasicInformationResponseDTO(
			tokenData.code,
			tokenData.token,
			tokenData.reference,
			tokenData.face_url,
			tokenData.full_name,
			tokenData.first_name,
			tokenData.middle_name,
			tokenData.last_name,
			tokenData.suffix,
			tokenData.gender,
			tokenData.marital_status,
			tokenData.blood_type,
			tokenData.email,
			tokenData.mobile_number,
			tokenData.birth_date,
			tokenData.full_address,
			tokenData.address_line_1,
			tokenData.address_line_2,
			tokenData.barangay,
			tokenData.municipality,
			tokenData.province,
			tokenData.country,
			tokenData.postal_code,
			tokenData.present_full_address,
			tokenData.present_address_line_1,
			tokenData.present_address_line_2,
			tokenData.present_barangay,
			tokenData.present_municipality,
			tokenData.present_province,
			tokenData.present_country,
			tokenData.present_postal_code,
			tokenData.residency_status,
			tokenData.place_of_birth,
			tokenData.pob_municipality,
			tokenData.pob_province,
			tokenData.pob_country
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
