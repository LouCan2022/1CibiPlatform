
namespace PhilSys.Services;
public class UpdateFaceLivenessSessionService
{
	private readonly HttpClient _httpClient;
	private readonly IPhilSysRepository _philSysRepository;
	private readonly ILogger<UpdateFaceLivenessSessionService> _logger;
	private readonly PostBasicInformationService _postBasicInformationService;
	private readonly PostPCNService _postPCNService;
	private readonly GetTokenService _getTokenService;
	private string client_id = "9ffe0ab6-1be1-47a8-bd3a-8560f1652a1a";
	private string client_secret = "YnQpGs34mdlH2bumAzzEhRc0pJXAjfcX8qBSDZyMtdiU4HDVwx4SAsIFLuLxHt51"; //use configuration later

	public UpdateFaceLivenessSessionService(
		IHttpClientFactory httpClientFactory,
		IPhilSysRepository philSysRepository,
		ILogger<UpdateFaceLivenessSessionService> logger,
		PostBasicInformationService PostBasicInformationService,
		PostPCNService PostPCNService,
		GetTokenService GetTokenService)
	{
		_httpClient = httpClientFactory.CreateClient("IDVClient");
		_philSysRepository = philSysRepository;
		_logger = logger;
		_postBasicInformationService = PostBasicInformationService;
		_postPCNService = PostPCNService;
		_getTokenService = GetTokenService;
	}
	public async Task<BasicInformationOrPCNResponseDTO> UpdateFaceLivenessSessionAsync(
		string HashToken,
		string FaceLivenessSessionId,
		CancellationToken ct = default
		)
	{
		_logger.LogInformation("Updating Face Liveness Session for HashToken: {HashToken}", HashToken);

		var result = await _philSysRepository.UpdateFaceLivenessSessionAsync(HashToken, FaceLivenessSessionId);
		if (result == null)
		{
			_logger.LogInformation("wdd updated Face Liveness Session for Tid: {Tid}", HashToken);
			return null!;
		}

		_logger.LogInformation("Successfully updated Face Liveness Session for Tid: {Tid}", HashToken);

		var token = await _getTokenService.GetPhilsysTokenAsync(client_id, client_secret);
		string accessToken = token.access_token;

		
		if (result.InquiryType!.Equals("name_dob", StringComparison.CurrentCultureIgnoreCase))
		{
			
			var responseBody = await _postBasicInformationService.PostBasicInformationAsync(result.FirstName!, result.MiddleName!, result.LastName!, result.Suffix!, result.BirthDate!, accessToken, FaceLivenessSessionId);
			await UpdateTransactionStatus(HashToken);
			return responseBody!;
		}

		if (result.InquiryType.Equals("pcn", StringComparison.OrdinalIgnoreCase))
		{
			
			var responseBody = await _postPCNService.PostPCNAsync(result.PCN!, accessToken, result.FaceLivenessSessionId!);
			await UpdateTransactionStatus(HashToken);
			return responseBody!;
		}
		
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
				error: "",
				message: "",
				error_description: ""
			);
	}

	public async Task UpdateTransactionStatus(string HashToken)
	{
		var existingTransaction = await _philSysRepository.GetTransactionDataByTidAsync(HashToken);

		if (existingTransaction == null)
		{
			_logger.LogWarning("Transaction with HashToken: {HashToken} not found.", HashToken);
			throw new Exception("No Transaction record found for this hashtoken.");
		}

		await _philSysRepository.UpdateTransactionDataAsync(existingTransaction);
	}
}
