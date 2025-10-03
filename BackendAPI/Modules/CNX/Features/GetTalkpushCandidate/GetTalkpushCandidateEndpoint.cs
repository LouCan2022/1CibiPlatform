namespace CNX.Features.GetTalkpushCandidate;

public record GetTalkpushCandidateRequest(string request) : ICommand<GetTalkpushCandidateResponse>;

public record GetTalkpushCandidateResult(List<CandidateResponseDto> CandidateResponseDto);


public class GetTalkpushCandidateEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("GetTalkpushCandidate", async (string request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new GetTalkpushCandidateCommmand(request);

            GetTalkpushCandidateResponse result = await sender.Send(command, cancellationToken);

            var response = new GetTalkpushCandidateResult(result.CandidateResponseDto);

            return Results.Ok(response.CandidateResponseDto);
        })
          .WithName("GetTalkpushCandidate")
          .WithTags("CNX")
          .Produces<GetTalkpushCandidateResult>()
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .WithSummary("Get Talkpush Candidate")
          .WithDescription("Get Talkpush Candidate")
          .RequireAuthorization();
    }
}