namespace CNX.Services;

public class GetCandidateService
{
    private readonly HttpClient _httpClient;

    public GetCandidateService(HttpClient httpClient)
    {
        this._httpClient = httpClient;
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
