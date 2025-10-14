namespace CNX.Features.GetTalkpushCandidate;

public record GetTalkpushCandidateCommmand(string request) : ICommand<GetTalkpushCandidateResult>;

public record GetTalkpushCandidateResult(List<CandidateResponseDto> CandidateResponseDto);

public class GetTalkpushCandidateHandler : ICommandHandler<GetTalkpushCandidateCommmand, GetTalkpushCandidateResult>
{
    private readonly GetCandidateService _getCandidateService;
    private readonly ILogger<GetTalkpushCandidateHandler> _logger;
    public GetTalkpushCandidateHandler(
        GetCandidateService getCandidateService,
        ILogger<GetTalkpushCandidateHandler> logger)
    {
        this._getCandidateService = getCandidateService;
        this._logger = logger;
    }
    public async Task<GetTalkpushCandidateResult> Handle(
        GetTalkpushCandidateCommmand request,
        CancellationToken cancellationToken)
    {
        var searchData = request.request;

        _logger.LogInformation("Handling GetTalkpushCandidateRequest - Search Data: {searchData}", searchData);

        var candidate = await _getCandidateService.GetCampaignInvitationsAsync(searchData, cancellationToken);

        if (candidate == null)
        {
            _logger.LogWarning("No candidate found for Search Data: {searchData}", searchData);

            throw new NotFoundException("Candidate not found.");
        }

        return new GetTalkpushCandidateResult(candidate);
    }
}