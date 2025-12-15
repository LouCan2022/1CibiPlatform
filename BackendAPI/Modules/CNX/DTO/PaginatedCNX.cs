namespace CNX.DTO;

public record PaginatedCNX
{
	public int Total { get; set; }
	public int Current_Page { get; set; }
	public int Pages { get; set; }
	public List<CandidateResponseDto>? Candidate { get; set; }
}
