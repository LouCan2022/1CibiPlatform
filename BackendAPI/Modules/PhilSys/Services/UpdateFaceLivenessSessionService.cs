namespace PhilSys.Services;

public class UpdateFaceLivenessSessionService
{
	private readonly HttpClient _httpClient;
	private readonly IPhilSysRepository _philSysRepository;
	private readonly IPhilSysResultRepository _philSysResultRepository;
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
		IPhilSysResultRepository philSysResultRepository,
		ILogger<UpdateFaceLivenessSessionService> logger,
		PostBasicInformationService PostBasicInformationService,
		PostPCNService PostPCNService,
		GetTokenService GetTokenService,
		IConfiguration configuration)
	{
		_httpClient = httpClientFactory.CreateClient("IDVClient");
		_philSysRepository = philSysRepository;
		_philSysResultRepository = philSysResultRepository;
		_logger = logger;
		_postBasicInformationService = PostBasicInformationService;
		_postPCNService = PostPCNService;
		_getTokenService = GetTokenService;
		_configuration = configuration;
		client_id = _configuration["PhilSys:ClientID"] ?? "";
		client_secret = _configuration["PhilSys:ClientSecret"] ?? "";
	}

	public async Task<VerificationResponseDTO> UpdateFaceLivenessSessionAsync(
		string HashToken,
		string FaceLivenessSessionId,
		CancellationToken ct = default
		)
	{
		CredentialResponseDTO? token = null;
		BasicInformationOrPCNResponseDTO responseBody = null!;

		_logger.LogInformation("Updating Face Liveness Session for Token: {HashToken}", HashToken);

		var result = await _philSysRepository.UpdateFaceLivenessSessionAsync(HashToken, FaceLivenessSessionId);
		if (result == null)
		{
			_logger.LogWarning("No transaction found for HashToken: {HashToken}. Unable to update Face Liveness Session.", HashToken);
			throw new InternalServerException($"No transaction record found for HashToken: {HashToken}. Face Liveness Session update aborted.");
		}

		_logger.LogInformation("Successfully updated Face Liveness Session for Token: {HashToken}", HashToken);

		try
		{
			token = await _getTokenService.GetPhilsysTokenAsync(client_id, client_secret);
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError("PhilSys token request failed at the calling function. {Exception}", ex);
			throw new InternalServerException("PhilSys token request for verification process failed. Please contact the administrator.");
		}

		string accessToken = token!.access_token;

		if (result!.InquiryType!.Equals("name_dob", StringComparison.CurrentCultureIgnoreCase))
		{
			try
			{
				responseBody = await _postBasicInformationService.PostBasicInformationAsync(result.FirstName!, result.MiddleName!, result.LastName!, result.Suffix!, result.BirthDate!, accessToken, FaceLivenessSessionId);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError("Basic Information request failed: {Exception}", ex);
				throw new InternalServerException("Basic Information request failed. Please contact the administrator.");
			}

			var convertedResponse = ConvertVerificationResponseDTO(result.Tid, responseBody!);

			await SendToClientWebHookAsync(result.WebHookUrl!, convertedResponse);

			await UpdateTransactionStatus(HashToken);

			await AddConvertedResponseToDbAsync(convertedResponse);

			return convertedResponse!;
		}

		if (result.InquiryType.Equals("pcn", StringComparison.OrdinalIgnoreCase))
		{
			try
			{
				responseBody = await _postPCNService.PostPCNAsync(result.PCN!, accessToken, result.FaceLivenessSessionId!);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError("PhilSys Card Number request failed: {Exception}", ex);
				throw new InternalServerException("PhilSys Card Number request failed. Please contact the administrator.");
			}

			var convertedResponse = ConvertVerificationResponseDTO(result.Tid, responseBody!);

			await SendToClientWebHookAsync(result.WebHookUrl!, convertedResponse);

			await UpdateTransactionStatus(HashToken);

			await AddConvertedResponseToDbAsync(convertedResponse);

			return convertedResponse!;
		}

		return new VerificationResponseDTO { };

	}

	private async Task UpdateTransactionStatus(string HashToken)
	{
		var existingTransaction = await _philSysRepository.GetTransactionDataByHashTokenAsync(HashToken);

		if (existingTransaction == null)
		{
			_logger.LogWarning("Transaction with HashToken: {HashToken} not found.", HashToken);
			throw new InternalServerException("No Transaction record found for this transaction. Please contact the administrator.");
		}

		var updateStatus = await _philSysRepository.UpdateTransactionDataAsync(existingTransaction);

		if (updateStatus == null)
		{
			_logger.LogError("Failed to Update the Transaction Status for {HashToken}.", HashToken);
		}

		_logger.LogInformation("Successfully Updated the Transaction Status.");
	}

	private async Task SendToClientWebHookAsync (string WebHook, VerificationResponseDTO VerificationResponseDTO)
	{
		if (WebHook != "/")
		{
			var clientResponse = await _httpClient.PostAsJsonAsync(WebHook, VerificationResponseDTO);
			if (!clientResponse.IsSuccessStatusCode)
			{
				_logger.LogError("Failed to send verification response to client webhook: {WebHook}. Status Code: {StatusCode}. Response Body: {ResponseBody}", 
							      WebHook, clientResponse.StatusCode, clientResponse);
				throw new InternalServerException($"Failed to send verification response to client's webhook: {WebHook}. Please contact the administrator.");
			}

			_logger.LogInformation("Successfully send the verification response to client webhook.");
		}
	}

	private async Task AddConvertedResponseToDbAsync(VerificationResponseDTO VerificationResponseDTO)
	{
		var philsysTransactionResult = VerificationResponseDTO.Adapt<PhilSysTransactionResult>();
		var result = await _philSysResultRepository.AddTransactionResultDataAsync(philsysTransactionResult);
		if (result == false)
		{
			_logger.LogError("Failed to Add the Converted Response in PhilSys Transaction Results' Table.");
		}
		_logger.LogInformation("Successfully Added the Converted Response in PhilSys Transaction Results' Table.");
	}

	private static VerificationResponseDTO ConvertVerificationResponseDTO(Guid Tid, BasicInformationOrPCNResponseDTO BasicInformationOrPCNResponseDTO)
	{
		if (string.IsNullOrEmpty(BasicInformationOrPCNResponseDTO.reference) )
		{
			return new VerificationResponseDTO
			{
				idv_session_id = Tid.ToString(),
				verified = false
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

		