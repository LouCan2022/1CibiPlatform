namespace CNX.DTO;

public class CampaignInvitationResponseDTO
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("current_page")]
    public string CurrentPage { get; set; } = "1";

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; } = [];
}

public class Candidate
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("candidate_id")]
    public int CandidateId { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("user_phone_number")]
    public string? UserPhoneNumber { get; set; }

    [JsonPropertyName("school_name")]
    public string? SchoolName { get; set; }

    [JsonPropertyName("education")]
    public string? Education { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("campaign_title")]
    public string? CampaignTitle { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("folder")]
    public string? Folder { get; set; }

    [JsonPropertyName("campaign_id")]
    public int CampaignId { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("others")]
    public OthersData? Others { get; set; }

    [JsonPropertyName("labels")]
    public List<string> Labels { get; set; } = [];

	[JsonPropertyName("documents")]
	public List<Documents> Documents { get; set; } = [];

	public BIForm BIForm { get; set; } = new BIForm();
	public string InitialReportDate { get; set; } = string.Empty;
	public string FinalReportDate { get; set; } = string.Empty;
}

public class Documents
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("created_at")]
	public string? Created_At { get; set; }

	[JsonPropertyName("verification_status")]
	public string? Verification_Status { get; set; }

	[JsonPropertyName("tag")]
	public string? Tag { get; set; }

	[JsonPropertyName("is_verifiable")]
	public bool Is_Verifiable { get; set; }

	[JsonPropertyName("files")]
	public List<DocumentFile>? Files { get; set; }
}


public class DocumentFile
{
	[JsonPropertyName("id")]
	public int Id { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("content_type")]
	public string? Content_Type { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("ocr_verified")]
	public string? Ocr_Verified { get; set; }

	[JsonPropertyName("created_at")]
	public string? Created_At { get; set; }
}

public class OthersData
{
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("date_of_birth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("marital_status")]
    public string? MaritalStatus { get; set; }

    [JsonPropertyName("package_account_name")]
    public string? PackageAccountName { get; set; }

    [JsonPropertyName("msa")]
    public string? Msa { get; set; }

    [JsonPropertyName("job_requisition_primary_location")]
    public string? JobRequisitionPrimaryLocation { get; set; }

    [JsonPropertyName("hire_date")]
    public string? HireDate { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("address_line_1")]
    public string? AddressLine1 { get; set; }

    [JsonPropertyName("sss_number")]
    public string? SssNumber { get; set; }

    [JsonPropertyName("extracted_sss_number")]
    public string? ExtractedSssNumber { get; set; }

    [JsonPropertyName("tin_number")]
    public string? TinNumber { get; set; }

    [JsonPropertyName("extracted_tin_number")]
    public string? ExtractedTinNumber { get; set; }

    [JsonPropertyName("philhealth_number")]
    public string? PhilhealthNumber { get; set; }

    [JsonPropertyName("extracted_philhealth_number")]
    public string? ExtractedPhilhealthNumber { get; set; }

    [JsonPropertyName("pag_ibig_number")]
    public string? PagIbigNumber { get; set; }

    [JsonPropertyName("extracted_pag-ibig_number")]
    public string? ExtractedPagIbigNumber { get; set; }

    [JsonPropertyName("job_requisition_id")]
    public string? JobRequisitionId { get; set; }

    [JsonPropertyName("bi_check")]
    public string? BiCheck { get; set; }
}