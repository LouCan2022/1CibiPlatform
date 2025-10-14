

namespace PhilSys.DTO;

public record PartnerSystemRequestDTO(
	[property: JsonPropertyName("external_user_id")] string ExternalUserId,
	[property: JsonPropertyName("callback_url")] string CallbackUrl,
	[property: JsonPropertyName("inquiry_type")] string InquiryType,
	[property: JsonPropertyName("identity_data")] IdentityData IdentityData
);

public record IdentityData(
	[property: JsonPropertyName("first_name")] string FirstName,
	[property: JsonPropertyName("middle_name")] string MiddleName,
	[property: JsonPropertyName("last_name")] string LastName,
	[property: JsonPropertyName("suffix")] string? Suffix,
	[property: JsonPropertyName("birth_date")] string BirthDate,
	[property: JsonPropertyName("pcn")] string? PCN
);