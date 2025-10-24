namespace PhilSys.DTO;

public record PartnerSystemResponseDTO(
	string? liveness_link,
	bool? isTransacted
);
