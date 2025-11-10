namespace CNX.Features.GetTalkpushCandidate;

public record GetTalkpushCandidateRequest(string request);

public record GetTalkpushCandidateResponse(List<CandidateResponseDto> CandidateResponseDto);


public class GetTalkpushCandidateEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("cnx/gettalkpushcandidate", async (string request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new GetTalkpushCandidateCommmand(request);

			GetTalkpushCandidateResult result = await sender.Send(command, cancellationToken);

			var response = new GetTalkpushCandidateResponse(result.CandidateResponseDto);

			return Results.Ok(response.CandidateResponseDto);
		})
		  .WithName("GetTalkpushCandidate")
		  .WithTags("CNX")
		  .Produces<GetTalkpushCandidateResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Get Talkpush Candidate")
		  .WithDescription("Get Talkpush Candidate")
		  .RequireAuthorization();
	}
}