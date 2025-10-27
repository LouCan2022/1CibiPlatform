namespace PhilSys.DTO;

public record PartnerSystemResponseDTO(
	string? idv_session_id,
	string? liveness_link,
	bool? isTransacted
);
