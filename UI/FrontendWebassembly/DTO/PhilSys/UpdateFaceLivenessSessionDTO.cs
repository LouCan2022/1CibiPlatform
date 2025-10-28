namespace FrontendWebassembly.DTO.PhilSys;

public record UpdateFaceLivenessSessionResponseDTO
{
	public string? idv_session_id { get; set; }
	public bool verified { get; set; }
	public DataSubject? data_subject { get; set; }
	public string? error { get; set; }
	public string? message { get; set; }
	public string? error_description { get; set; }
}
public record DataSubject
{
	public string? digital_id { get; set; }
	public string? national_id_number { get; set; }
	public string? face_image_url { get; set; }
	public string? full_name { get; set; }
	public string? first_name { get; set; }
	public string? middle_name { get; set; }
	public string? last_name { get; set; }
	public string? suffix { get; set; }
	public string? gender { get; set; }
	public string? marital_status { get; set; }
	public string? birth_date { get; set; }
	public string? email { get; set; }
	public string? mobile_number { get; set; }
	public string? blood_type { get; set; }
	public Address? address { get; set; }
	public PlaceOfBirth? place_of_birth { get; set; }
}

public record Address
{
	public string? permanent { get; set; }
	public string? present { get; set; }
}

public record PlaceOfBirth
{
	public string? full { get; set; }
	public string? municipality { get; set; }
	public string? province { get; set; }
	public string? country { get; set; }
}

