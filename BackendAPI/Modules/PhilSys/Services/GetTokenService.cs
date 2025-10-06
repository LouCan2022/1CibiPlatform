namespace PhilSys.Services;

public class GetTokenService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GetTokenService> _logger;
    private readonly IConfiguration _configuration;

    public GetTokenService(
        IHttpClientFactory httpClientFactory,
        ILogger<GetTokenService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("PhilSys");
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<CredentialResponseDTO> GetPhilsysTokenAsync(
        string clientId,
        string clientSecret,
        CancellationToken ct = default)
    {
        var endpoint = "auth";

        var body = new
        {
            client_id = clientId,
            client_secret = clientSecret
        };

        _logger.LogInformation("Sending token request to PhilSys endpoint: {Endpoint}", endpoint);

        var response = await SendRequestAsync(endpoint, body, ct);

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("PhilSys Token Response: {Response}", responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("PhilSys token request failed: {Status} - {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"PhilSys token request failed: {response.StatusCode} - {responseBody}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        var data = jsonDoc.RootElement.GetProperty("data");

        return new CredentialResponseDTO(
            data.GetProperty("access_token").GetString()!,
            data.GetProperty("token_type").GetString()!,
            data.GetProperty("expires_at").GetString()!
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
