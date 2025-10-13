namespace PhilSys.Features.PostPCN;

public record PostPCNRequest(int value, string face_liveness_session_id) : ICommand<PostPCNResponse>;

public record PostPCNResult(BasicInformationOrPCNResponseDTO PCNResponseDTO);
public class PostPCNEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("PostPCN", async (PostPCNRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new PostPCNCommand(
				request.value,
				request.face_liveness_session_id
				);

			PostPCNResponse result = await sender.Send(command, cancellationToken);

			var response = new PostPCNResult(result.PCNResponseDTO);

			return Results.Ok(response.PCNResponseDTO);
		})
		.WithName("PostPCN")
		.WithTags("PhilSys")
		.Produces<PostPCNResult>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Retrieve If Verified")
		.WithDescription("Retrieves an the verify response from the PhilSys API using client credentials.");
	}
}
