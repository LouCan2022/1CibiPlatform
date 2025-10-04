namespace CNX.Services;

public class GetCandidateService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GetCandidateService> _logger;
    private readonly IConfiguration _configuration;

    public GetCandidateService(
        IHttpClientFactory factory,
        ILogger<GetCandidateService> logger,
        IConfiguration configuration)
    {
        this._httpClient = factory.CreateClient("Talkpush");
        this._logger = logger;
        this._configuration = configuration;
    }


    public async Task<List<CandidateResponseDto>?> GetCampaignInvitationsAsync(
        string searchData,
        CancellationToken ct = default)
    {
        var apiKey = _configuration.GetValue<string>("CNXTalkpushsKey:Key");
        var filterCheck = _configuration.GetValue<string>("CNXTalkpushsKey:FilterCheck");

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("API key is not configured.");
        }

        var requestURI = BuildRequestUri(searchData, apiKey, filterCheck);

        _logger.LogInformation("=== REQUEST ===");

        var response = await SendRequestAsync(requestURI, ct);

        _logger.LogInformation("=== RESPONSE ===");
        _logger.LogInformation("Status: {Status}", response.StatusCode);

        // READ THE ERROR RESPONSE BEFORE THROWING
        var responseBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogInformation("Body: {Body}", responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API returned error. Status: {Status}, Body: {Body}",
                response.StatusCode, responseBody);
            throw new HttpRequestException($"API Error: {response.StatusCode} - {responseBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<CampaignInvitationResponseDTO>(
            cancellationToken: ct);

        var mapDTO = MapToDtos(result!.Candidates);

        return mapDTO;
    }

    protected virtual async Task<HttpResponseMessage> SendRequestAsync(
        string requestUri,

        CancellationToken ct)
    {
        return await _httpClient.GetAsync(requestUri, ct);
    }

    private string BuildRequestUri(
        string searchData,
        string apiKey,
        string filterCheck
        )
    {
        var queryParams = new Dictionary<string, string>
        {
            ["api_key"] = apiKey,
            ["filter[query]"] = searchData,
            ["filter[others][bi_check]"] = filterCheck,
            ["page"] = "1"
        };

        return QueryHelpers.AddQueryString("campaign_invitations", queryParams!);
    }


    private static List<CandidateResponseDto> MapToDtos(List<Candidate> candidates)
    {
        return candidates.Select(c => new CandidateResponseDto(
            c.CandidateId,
            c.Others?.JobRequisitionId,
            c.FirstName,
            c.Others?.MiddleName,
            c.LastName,
            c.Others?.DateOfBirth,
            c.Email,
            c.UserPhoneNumber,
            c.Others?.MaritalStatus,
            c.Others?.PackageAccountName,
            c.CampaignTitle,
            c.Others?.Msa,
            c.Others?.JobRequisitionPrimaryLocation,
            c.Gender,
            c.Others?.HireDate,
            c.SchoolName,
            c.Education,
            c.Others?.City,
            c.Others?.PostalCode,
            c.Others?.AddressLine1,
            c.Others?.SssNumber,
            c.Others?.ExtractedSssNumber,
            c.Others?.TinNumber,
            c.Others?.ExtractedTinNumber,
            c.Others?.PhilhealthNumber,
            c.Others?.ExtractedPhilhealthNumber,
            c.Others?.PagIbigNumber,
            c.Others?.ExtractedPagIbigNumber)).ToList();
    }


}
