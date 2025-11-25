namespace FrontendWebassembly.DTO.PhilSys;

public record UpdateFaceLivenessSessionResponseDTO
{
	public string? idv_session_id { get; set; }
	public bool? verified { get; set; }
	public DataSubject? data_subject { get; set; }
	public string? trace_id { get; set; }
	public string? error_message { get; set; }
}
public record DataSubject
{
	public string? digital_id { get; init; }
	public string? national_id_number { get; init; }
	public string? face_url { get; init; }
	public string? full_name { get; init; }
	public string? first_name { get; init; }
	public string? middle_name { get; init; }
	public string? last_name { get; init; }
	public string? suffix { get; init; }
	public string? gender { get; init; }
	public string? marital_status { get; init; }
	public string? blood_type { get; init; }
	public string? email { get; init; }
	public string? mobile_number { get; init; }
	public string? birth_date { get; init; }
	public string? full_address { get; init; }
	public string? address_line_1 { get; init; }
	public string? address_line_2 { get; init; }
	public string? barangay { get; init; }
	public string? municipality { get; init; }
	public string? province { get; init; }
	public string? country { get; init; }
	public string? postal_code { get; init; }
	public string? present_full_address { get; init; }
	public string? present_address_line_1 { get; init; }
	public string? present_address_line_2 { get; init; }
	public string? present_barangay { get; init; }
	public string? present_municipality { get; init; }
	public string? present_province { get; init; }
	public string? present_country { get; init; }
	public string? present_postal_code { get; init; }
	public string? residency_status { get; init; }
	public string? place_of_birth { get; init; }
	public string? pob_municipality { get; init; }
	public string? pob_province { get; init; }
	public string? pob_country { get; init; }
}
