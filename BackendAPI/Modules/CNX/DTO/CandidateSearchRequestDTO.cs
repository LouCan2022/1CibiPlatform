
namespace CNX.DTO
{
    public record CandidateSearchQueryDTO(
        string? api_key = null,
        string? filter_others_bi_check = null,
        string? filter_query = null,
        int page = 1
    );
}
