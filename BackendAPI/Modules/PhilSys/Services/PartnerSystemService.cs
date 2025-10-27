namespace PhilSys.Services;

public class PartnerSystemService
{
	private readonly ILogger<PartnerSystemService> _logger;
	private readonly IPhilSysRepository _repository;
	private readonly IConfiguration _configuration;
	private readonly IHashService _hashService;
	private readonly ISecureToken _securetoken;
	private readonly double _livenessExpiryMinutes;
	public PartnerSystemService(
		ILogger<PartnerSystemService> logger, 
		IPhilSysRepository repository,
		IConfiguration configuration,
		IHashService hashService,
		ISecureToken securetoken)
	{
		_logger = logger;
		_repository = repository;
		_configuration = configuration;
		_hashService = hashService;
		_securetoken = securetoken;
		_livenessExpiryMinutes = int.Parse(_configuration["PhilSys:LivenessSessionExpiryInMinutes"] ?? "10");
	}
	public async Task<PartnerSystemResponseDTO> PartnerSystemQueryAsync(string callback_url, string inquiry_type, IdentityData identity_data)
	{
		
		PhilSysTransaction transaction = new PhilSysTransaction { } ;

		Guid tid;

		var token = _securetoken.GenerateSecureToken();

		var identifier = !string.IsNullOrWhiteSpace(identity_data.PCN)
							 ? identity_data.PCN
							 : $"{identity_data.FirstName} {identity_data.LastName}".Trim();

		if (token == null)
		{
			_logger.LogError("Failed to generate Token for identity: {Identifier}", identifier);
			throw new Exception("Failed to generate Token.");
		}

		var HashToken = _hashService.Hash(token);

		if (HashToken == null)
		{
			_logger.LogError("Failed to hash Token for identity: {Identifier}", identifier);
			throw new Exception("Failed to hash Token.");
		}

		if (inquiry_type.Equals("name_dob", StringComparison.OrdinalIgnoreCase))
		{
			transaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				InquiryType = "name_dob",
				FirstName = identity_data.FirstName,
				MiddleName = identity_data.MiddleName,
				LastName = identity_data.LastName,
				Suffix = identity_data.Suffix,
				BirthDate = identity_data.BirthDate,
				IsTransacted = false,
				HashToken = HashToken,
				WebHookUrl = callback_url,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_livenessExpiryMinutes)
			};
		}

		else if (inquiry_type.Equals("pcn", StringComparison.OrdinalIgnoreCase))
		{
			transaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				InquiryType = "pcn",
				PCN = identity_data.PCN,
				IsTransacted = false,
				HashToken = HashToken,
				WebHookUrl = callback_url,
				CreatedAt = DateTime.UtcNow,
				ExpiresAt = DateTime.UtcNow.AddMinutes(_livenessExpiryMinutes)
			};
		}

		var livenessUrl = $"http://localhost:5134/philsys/idv/liveness/{transaction.HashToken}";

		var result = await _repository.AddTransactionDataAsync(transaction);
		if (result == false)
			_logger.LogError("Failed to add transaction data for Tid: {Tid}", transaction.Tid);
			
	
		return new PartnerSystemResponseDTO(
			idv_session_id: transaction.Tid.ToString(),
			liveness_link: livenessUrl,
			isTransacted: false
		);
	}
}
