namespace PhilSys.Services;

public class GetTokenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GetTokenService> _logger;

    public GetTokenService(
        IHttpClientFactory httpClientFactory,
        ILogger<GetTokenService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("PhilSys");
        _logger = logger;
    }

    public async Task<string> GetPhilsysTokenAsync(
        string clientId,
        string clientSecret)
    {
        var body = new
        {
            client_id = clientId,
            client_secret = clientSecret
        };

    	_logger.LogInformation("Requesting PhilSys token with client_id: {ClientId}", clientId);

		var response = await SendRequestAsync("auth", body);

		var responseBody = await response.Content.ReadFromJsonAsync<PhilSysTokenResponse>();

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("PhilSys token request failed: {Status} - {Body}", response.StatusCode, responseBody);
			throw new HttpRequestException("PhilSys token request failed.");
		}

		_logger.LogInformation("Successful Request for Token.");

		var tokenData = responseBody!.data;

		return tokenData.access_token;
	}

	public virtual async Task<HttpResponseMessage> SendRequestAsync(
		string endpoint,
		object body)
	{
		return await _httpClient.PostAsJsonAsync(endpoint, body);
	}
}