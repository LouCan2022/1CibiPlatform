

namespace PhilSys.Services;

public class PartnerSystemService
{
	private readonly ILogger<PartnerSystemService> _logger;
	private readonly IPhilSysRepository _repository;

	public PartnerSystemService(
		ILogger<PartnerSystemService> logger, IPhilSysRepository repository)
	{
		_logger = logger;
		_repository = repository;
	}
	public async Task<PartnerSystemResponseDTO> PartnerSystemQueryAsync(PartnerSystemRequestDTO PartnerSystemRequestDTO)
	{
		
		PhilSysTransaction transaction = new PhilSysTransaction { } ;
		
		if (PartnerSystemRequestDTO.InquiryType.Equals("name_dob", StringComparison.OrdinalIgnoreCase))
		{
			transaction = new PhilSysTransaction
			{
				Tid = Guid.NewGuid(),
				InquiryType = "name_dob",
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
				InquiryType = "pcn",
				PCN = PartnerSystemRequestDTO.IdentityData.PCN,
				IsTransacted = false,
				CreatedAt = DateTime.UtcNow
			};
		}

		var livenessUrl = $"http://localhost:5134/philsys/idv/liveness/{transaction.Tid}";

		var result = await _repository.AddTransactionDataAsync(transaction);
		if (result == false)
			throw new InvalidOperationException("Failed");

		return new PartnerSystemResponseDTO(
			code: null,
			token: null,
			reference: null,
			face_url: null,
			full_name: null,
			first_name: null,
			middle_name: null,
			last_name: null,
			suffix: null,
			gender: null,
			marital_status: null,
			blood_type: null,
			email: null,
			mobile_number: null,
			birth_date: null,
			full_address: null,
			address_line_1: null,
			address_line_2: null,
			barangay: null,
			municipality: null,
			province: null,
			country: null,
			postal_code: null,
			present_full_address: null,
			present_address_line_1: null,
			present_address_line_2: null,
			present_barangay: null,
			present_municipality: null,
			present_province: null,
			present_country: null,
			present_postal_code: null,
			residency_status: null,
			place_of_birth: null,
			pob_municipality: null,
			pob_province: null,
			pob_country: null,
			liveness_link: livenessUrl,
			face_liveness_session_id: null,
			isTransacted: false
		);
	}
}
