namespace FrontendWebassembly.DTO.CNX;

public record CandidateResponseDTO
{
	public int CandidateId { get; set; }
	public string? JobRequisitionId { get; set; }
	public string? FirstName { get; set; }
	public string? MiddleName { get; set; }
	public string? LastName { get; set; }
	public string? DateOfBirth { get; set; }
	public string? Email { get; set; }
	public string? UserPhoneNumber { get; set; }
	public string? MaritalStatus { get; set; }
	public string? PackageAccountName { get; set; }
	public string? CampaignTitle { get; set; }
	public string? Msa { get; set; }
	public string? JobRequisitionPrimaryLocation { get; set; }
	public string? Gender { get; set; }
	public string? HireDate { get; set; }
	public string? SchoolName { get; set; }
	public string? Education { get; set; }
	public string? City { get; set; }
	public string? PostalCode { get; set; }
	public string? AddressLine1 { get; set; }
	public string? SssNumber { get; set; }
	public string? ExtractedSssNumber { get; set; }
	public string? TinNumber { get; set; }
	public string? ExtractedTinNumber { get; set; }
	public string? PhilhealthNumber { get; set; }
	public string? ExtractedPhilhealthNumber { get; set; }
	public string? PagIbigNumber { get; set; }
	public string? ExtractedPagIbigNumber { get; set; }
	public BIForm? BIForm { get; set; }
	public string? InitialBIReportDate { get; set; }
	public string? FinalBIReportDate { get; set; }
}

public class BIForm
{
	public string? FileName { get; set; }
	public string? FileURL { get; set; }
}

public record PaginatedCNX
{
	public int Total { get; set; }
	public int Current_Page { get; set; }
	public int Pages { get; set; }
	public List<CandidateResponseDTO>? Candidate { get; set; }
}
