namespace PhilSys.Features.PostPCN;

public record PostPCNRequest(string value,
							 string bearer_token,
							 string face_liveness_session_id) : ICommand<PostPCNResponse>;

public record PostPCNResponse(BasicInformationOrPCNResponseDTO PCNResponseDTO);
public class PostPCNEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("postpcn", async (PostPCNRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new PostPCNCommand(
				request.value,
				request.bearer_token,
				request.face_liveness_session_id
				);

			PostPCNResult result = await sender.Send(command, cancellationToken);

			var response = new PostPCNResponse(result.PCNResponseDTO);

			return Results.Ok(response.PCNResponseDTO);
		})
		.WithName("PostPCN")
		.WithTags("PhilSys")
		.Produces<PostPCNResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.ProducesProblem(StatusCodes.Status401Unauthorized)
		.WithSummary("Retrieve If Verified")
		.WithDescription("Retrieves an the verify response from the PhilSys API using client credentials.");
	}
}
