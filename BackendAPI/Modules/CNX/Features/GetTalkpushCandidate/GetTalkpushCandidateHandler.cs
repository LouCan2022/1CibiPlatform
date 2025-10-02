namespace CNX.Features.GetTalkpushCandidate;

public record GetTalkpushCandidateRequest(CandidateSearchQueryDTO request) : ICommand<GetTalkpushCandidateResponse>;

public record GetTalkpushCandidateResponse(CandidateResponseDto CandidateResponseDto);

