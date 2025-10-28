
namespace PhilSys.Services;
public class UpdateFaceLivenessSessionService
{
	private readonly HttpClient _httpClient;
	private readonly IPhilSysRepository _philSysRepository;
	private readonly ILogger<UpdateFaceLivenessSessionService> _logger;
	private readonly PostBasicInformationService _postBasicInformationService;
	private readonly PostPCNService _postPCNService;
	private readonly GetTokenService _getTokenService;
	private readonly IConfiguration _configuration;
	private readonly string client_id;
	private readonly string client_secret;

	public UpdateFaceLivenessSessionService(
		IHttpClientFactory httpClientFactory,
		IPhilSysRepository philSysRepository,
		ILogger<UpdateFaceLivenessSessionService> logger,
		PostBasicInformationService PostBasicInformationService,
		PostPCNService PostPCNService,
		GetTokenService GetTokenService,
		IConfiguration configuration)
	{
		_httpClient = httpClientFactory.CreateClient("IDVClient");
		_philSysRepository = philSysRepository;
		_logger = logger;
		_postBasicInformationService = PostBasicInformationService;
		_postPCNService = PostPCNService;
		_getTokenService = GetTokenService;
		_configuration = configuration;
		client_id = _configuration["PhilSys:ClientID"]!;
		client_secret = _configuration["PhilSys:ClientSecret"]!;
	}
	public async Task<VerificationResponseDTO> UpdateFaceLivenessSessionAsync(
		string HashToken,
		string FaceLivenessSessionId,
		CancellationToken ct = default
		)
	{
		_logger.LogInformation("Updating Face Liveness Session for Token: {HashToken}", HashToken);

		var result = await _philSysRepository.UpdateFaceLivenessSessionAsync(HashToken, FaceLivenessSessionId);
		if (result == null)
		{
			_logger.LogInformation("Failed to updated Face Liveness Session for Token: {HashToken}", HashToken);
			throw new Exception($"Error in Updating Face Livenession for Token: {HashToken}");
		}

		_logger.LogInformation("Successfully updated Face Liveness Session for Token: {HashToken}", HashToken);

		var token = await _getTokenService.GetPhilsysTokenAsync(client_id, client_secret);
		string accessToken = token.access_token;


		if (result!.InquiryType!.Equals("name_dob", StringComparison.CurrentCultureIgnoreCase))
		{

			var responseBody = await _postBasicInformationService.PostBasicInformationAsync(result.FirstName!, result.MiddleName!, result.LastName!, result.Suffix!, result.BirthDate!, accessToken, FaceLivenessSessionId);
			await UpdateTransactionStatus(HashToken);

			if (!string.IsNullOrEmpty(responseBody.error))
			{
				_logger.LogError("Error in PostBasicInformationAsync: {Error}", responseBody.error);
			}

			var convertedResponse = ConvertVerificationResponseDTO(result.Tid, responseBody!);

			if (result.WebHookUrl != "/")
			{
				var sendToClient = await _httpClient.PostAsJsonAsync(result.WebHookUrl, convertedResponse);
			}
			
			return convertedResponse!;
		}

		if (result.InquiryType.Equals("pcn", StringComparison.OrdinalIgnoreCase))
		{

			var responseBody = await _postPCNService.PostPCNAsync(result.PCN!, accessToken, result.FaceLivenessSessionId!);
			await UpdateTransactionStatus(HashToken);

			if (!string.IsNullOrEmpty(responseBody.error))
			{
				_logger.LogError("Error in PostPCNAsync: {Error}", responseBody.error);
			}

			var convertedResponse = ConvertVerificationResponseDTO(result.Tid, responseBody!);

			if (result.WebHookUrl != "/")
			{
				var sendToClient = await _httpClient.PostAsJsonAsync(result.WebHookUrl, convertedResponse);
			}

			return convertedResponse!;
		}

		return new VerificationResponseDTO { };

	}

	public async Task UpdateTransactionStatus(string HashToken)
	{
		var existingTransaction = await _philSysRepository.GetTransactionDataByHashTokenAsync(HashToken);

		if (existingTransaction == null)
		{
			_logger.LogWarning("Transaction with HashToken: {HashToken} not found.", HashToken);
			throw new Exception("No Transaction record found for this hashtoken.");
		}

		await _philSysRepository.UpdateTransactionDataAsync(existingTransaction);
	}

	public VerificationResponseDTO ConvertVerificationResponseDTO(Guid Tid, BasicInformationOrPCNResponseDTO BasicInformationOrPCNResponseDTO)
	{
		if (string.IsNullOrEmpty(BasicInformationOrPCNResponseDTO.reference))
		{
			return new VerificationResponseDTO
			{
				idv_session_id = Tid.ToString(),
				verified = false,
				error = BasicInformationOrPCNResponseDTO.error,
				message = BasicInformationOrPCNResponseDTO.message,
				error_description = BasicInformationOrPCNResponseDTO.error_description
			};
		}
		return new VerificationResponseDTO
		{
			idv_session_id = Tid.ToString(),
			verified = true,
			data_subject = new DataSubject
			{
				digital_id = BasicInformationOrPCNResponseDTO.code,
				national_id_number = BasicInformationOrPCNResponseDTO.reference,
				face_image_url = BasicInformationOrPCNResponseDTO.face_url,
				full_name = BasicInformationOrPCNResponseDTO.full_name,
				first_name = BasicInformationOrPCNResponseDTO.first_name,
				middle_name = BasicInformationOrPCNResponseDTO.middle_name,
				last_name = BasicInformationOrPCNResponseDTO.last_name,
				suffix = BasicInformationOrPCNResponseDTO.suffix,
				gender = BasicInformationOrPCNResponseDTO.gender,
				marital_status = BasicInformationOrPCNResponseDTO.marital_status,
				birth_date = BasicInformationOrPCNResponseDTO.birth_date,
				email = BasicInformationOrPCNResponseDTO.email,
				mobile_number = BasicInformationOrPCNResponseDTO.mobile_number,
				blood_type = BasicInformationOrPCNResponseDTO.blood_type,
				address = new Address
				{
					permanent = BasicInformationOrPCNResponseDTO.full_address,
					present = BasicInformationOrPCNResponseDTO.present_full_address
				},
				place_of_birth = new PlaceOfBirth
				{
					full = BasicInformationOrPCNResponseDTO.place_of_birth,
					municipality = BasicInformationOrPCNResponseDTO.pob_municipality,
					province = BasicInformationOrPCNResponseDTO.pob_province,
					country = BasicInformationOrPCNResponseDTO.pob_country
				}
			}
		};
	}
}

		