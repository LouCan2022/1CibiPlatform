namespace FrontendWebassembly.DTO.Auth;

public record CredErrorResponseDTO(string Title,
	int Status,
	string Detail,
	string Instance,
	string TraceId);
