using PhilSys.Data.Repository;

namespace PhilSys.Services;

public class PartnerSystemService
{
	private readonly HttpClient _httpClientFactory;
	private readonly ILogger<PartnerSystemService> _logger;
	private readonly IPhilSysRepository _repository;

	public PartnerSystemService(
		IHttpClientFactory httpClientFactory,
		ILogger<PartnerSystemService> logger, IPhilSysRepository repository)
	{
		_httpClientFactory = httpClientFactory.CreateClient("PhilSys");
		_logger = logger;
		_repository = repository;
	}
	public async Task<PartnerSystemResponseDTO> PartnerSystemQueryAsync(PartnerSystemRequestDTO PartnerSystemRequestDTO)
	{
		
		PhilSysTransaction transaction = new PhilSysTransaction { } ;
		var livenessUrl = $"http://localhost:5123/philsys/idv/liveness?tid={transaction.Tid}";
		if (PartnerSystemRequestDTO.InquiryType.Equals("name_dob", StringComparison.OrdinalIgnoreCase))
		{
			transaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				FirstName = PartnerSystemRequestDTO.IdentityData.FirstName,
				MiddleName = PartnerSystemRequestDTO.IdentityData.MiddleName,
				LastName = PartnerSystemRequestDTO.IdentityData.LastName,
				Suffix = PartnerSystemRequestDTO.IdentityData.Suffix,
				BirthDate = PartnerSystemRequestDTO.IdentityData.BirthDate,
				IsTransacted = false,
				CreatedAt = DateTime.UtcNow
			};
		}
		else if (PartnerSystemRequestDTO.InquiryType.Equals("pcn", StringComparison.OrdinalIgnoreCase))
		{
			transaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				PCN = PartnerSystemRequestDTO.IdentityData.PCN,
				IsTransacted = false,
				CreatedAt = DateTime.UtcNow
			};
		}

		var result = await _repository.AddTransactionDataAsync(transaction);
		if (result == false)
			return new PartnerSystemResponseDTO { };


		

		
	}
}
