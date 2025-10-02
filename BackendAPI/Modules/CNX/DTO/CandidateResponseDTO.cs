namespace CNX.DTO;


public record CandidateResponseDto(
    int CandidateId,
    string? JobRequisitionId = null,
    string? FirstName = null,
    string? MiddleName = null,
    string? LastName = null,
    string? DateOfBirth = null,
    string? Email = null,
    string? UserPhoneNumber = null,
    string? MaritalStatus = null,
    string? PackageAccountName = null,
    string? CampaignTitle = null,
    string? Msa = null,
    string? JobRequisitionPrimaryLocation = null,
    string? Gender = null,
    string? HireDate = null,
    string? SchoolName = null,
    string? Education = null,
    string? City = null,
    string? PostalCode = null,
    string? AddressLine1 = null,
    string? SssNumber = null,
    string? ExtractedSssNumber = null,
    string? TinNumber = null,
    string? ExtractedTinNumber = null,
    string? PhilhealthNumber = null,
    string? ExtractedPhilhealthNumber = null,
    string? PagIbigNumber = null,
    string? ExtractedPagIbigNumber = null
);