namespace FrontendWebassembly.DTO.SharedDTO;

public record ApiErrorResponse(
   string Title,
   int Status,
   string Detail,
   string Instance,
   string TraceId
);

