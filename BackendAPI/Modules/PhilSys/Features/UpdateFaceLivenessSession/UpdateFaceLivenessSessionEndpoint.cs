namespace PhilSys.Features.PostFaceLivenessSession;

public record UpdateFaceLivenessSessionRequest(string HashToken, string FaceLivenessSessionId) : ICommand<UpdateFaceLivenessSessionResponse>;

public record UpdateFaceLivenessSessionResponse(BasicInformationOrPCNResponseDTO BasicInformationOrPCNResponseDTO);
public class UpdateFaceLivenessSessionEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("updatefacelivenesssession", async (UpdateFaceLivenessSessionRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new UpdateFaceLivenessSessionCommand(
				request.HashToken,
				request.FaceLivenessSessionId
				);
			UpdateFaceLivenessSessionResult result = await sender.Send(command, cancellationToken);

			var response = new UpdateFaceLivenessSessionResponse(result.BasicInformationOrPCNResponseDTO);

			return Results.Ok(response.BasicInformationOrPCNResponseDTO);
		})
		.WithName("UpdateFaceLivenessSession")
		.WithTags("PhilSys")
		.Produces<UpdateFaceLivenessSessionResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.ProducesProblem(StatusCodes.Status401Unauthorized)
		.WithSummary("Update Face Liveness Session Id");
	}
}
