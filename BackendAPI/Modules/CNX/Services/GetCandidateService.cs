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

	public async Task<PaginatedCNX> GetCampaignInvitationsAsync(
		string searchData,
		string page,
		CancellationToken ct = default)
	{
		PaginatedCNX candidatesDTO = new();
		var apiKey = _configuration.GetValue<string>("CNXTalkpushsKey:Key");
		var filterCheck = _configuration.GetValue<string>("CNXTalkpushsKey:FilterCheck");
		BIForm? BIForm = null;
		string? InitialReportDate = null;
		string? FinalReportDate = null;

		if (string.IsNullOrEmpty(apiKey))
		{
			throw new InvalidOperationException("API key is not configured.");
		}

		var requestURI = BuildRequestUri(searchData, apiKey, filterCheck, page);

		_logger.LogInformation("=== REQUEST ===");

		var response = await SendRequestAsync(requestURI, ct);

		_logger.LogInformation("=== RESPONSE ===");
		_logger.LogInformation("Status: {Status}", response.StatusCode);

		var responseBody = await response.Content.ReadAsStringAsync(ct);
		_logger.LogInformation("Body: {Body}", responseBody);

		if (!response.IsSuccessStatusCode)
		{
			_logger.LogError("API returned error. Status: {Status}, Body: {Body}",
				response.StatusCode, responseBody);
			throw new HttpRequestException($"API Error: {response.StatusCode} - {responseBody}");
		}

		var result = await response.Content.ReadFromJsonAsync<CampaignInvitationResponseDTO>(cancellationToken: ct);

		if (result?.Candidates != null)
		{
			var allowedTags = new[] { "BI Form", "Initial BI Report", "Final BI Report" };

			foreach (var candidate in result.Candidates)
			{
				candidate.Documents = candidate.Documents?
					.Where(d => !string.IsNullOrWhiteSpace(d.Tag) && allowedTags.Contains(d.Tag))
					.ToList();

				string? candidateBIFormFileName = "";
				string? candidateBIFormFileUrl = "";
				string candidateInitialReportDate = "";
				string candidateFinalReportDate = "";

				var biFormDoc = candidate.Documents?.FirstOrDefault(d => d.Tag == "BI Form");
				if (biFormDoc != null)
				{
					var biFormFile = biFormDoc.Files?.FirstOrDefault();
					if (biFormFile != null)
					{
						candidateBIFormFileName = biFormFile.Name;
						candidateBIFormFileUrl = biFormFile.Url;
					}
				}

				var initialBIReport = candidate.Documents?.FirstOrDefault(d => d.Tag == "Initial BI Report");
				if (initialBIReport?.Files != null && initialBIReport.Files.Any())
				{
					var initialFile = initialBIReport.Files.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.Created_At));
					if (initialFile != null)
					{
						candidateInitialReportDate = LocalTimeConverter(initialFile.Created_At!); 
					}
				}

				var finalBIReport = candidate.Documents?.FirstOrDefault(d => d.Tag == "Final BI Report");
				if (finalBIReport?.Files != null && finalBIReport.Files.Any())
				{
					var finalFile = finalBIReport.Files.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.Created_At));
					if (finalFile != null)
					{
						candidateFinalReportDate = LocalTimeConverter(finalFile.Created_At!);
					}
				}

				candidate.BIForm = new BIForm
				{
					FileName = candidateBIFormFileName,
					FileURL = candidateBIFormFileUrl
				};

				candidate.InitialReportDate = candidateInitialReportDate;
				candidate.FinalReportDate = candidateFinalReportDate;
			}

			var mapDTO = MapToDtos(result); 
			candidatesDTO.Total = result.Total;
			candidatesDTO.Current_Page = result.Pages;
			candidatesDTO.Total = result.Total;
			candidatesDTO.Candidate = mapDTO;
		}
		return candidatesDTO!;
	}


	private string LocalTimeConverter(string date)
	{
		DateTimeOffset dto = DateTimeOffset.Parse(date);

		DateTime localTime = dto.ToLocalTime().DateTime;

		return localTime.ToString();
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
        string filterCheck,
		string page
        )
    {
        var queryParams = new Dictionary<string, string>
        {
            ["api_key"] = apiKey,
            ["filter[query]"] = searchData,
            ["filter[others][bi_check]"] = filterCheck,
            ["page"] = page,
			["include_documents"] = "true"
		};

        return QueryHelpers.AddQueryString("campaign_invitations", queryParams!);
    }

    private static List<CandidateResponseDto> MapToDtos(CampaignInvitationResponseDTO candidates)
    {
        return candidates.Candidates.Select(c => new CandidateResponseDto(
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
            c.Others?.ExtractedPagIbigNumber,
			c.BIForm,
			c.InitialReportDate,
			c.FinalReportDate
			)).ToList();
    }
}
