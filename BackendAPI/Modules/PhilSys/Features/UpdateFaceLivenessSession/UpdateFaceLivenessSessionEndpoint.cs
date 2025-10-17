
using PhilSys.Features.PostPCN;
using PhilSys.Features.UpdateFaceLivenessSession;

namespace PhilSys.Features.PostFaceLivenessSession;

public record UpdateFaceLivenessSessionRequest(Guid Tid, string FaceLivenessSessionId) : ICommand<UpdateFaceLivenessSessionResponse>;

public record UpdateFaceLivenessSessionResponse(bool Success);
public class UpdateFaceLivenessSessionEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("updatefacelivenesssession", async (UpdateFaceLivenessSessionRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new UpdateFaceLivenessSessionCommand(
				request.Tid,
				request.FaceLivenessSessionId
				);
			UpdateFaceLivenessSessionResult result = await sender.Send(command, cancellationToken);
			var response = new UpdateFaceLivenessSessionResponse(result.Success);
			return Results.Ok(response);
		})
		.WithName("UpdateFaceLivenessSession")
		.WithTags("PhilSys")
		.Produces<PostPCNResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.ProducesProblem(StatusCodes.Status401Unauthorized)
		.WithSummary("Update Face Liveness Session Id");
	}
}
