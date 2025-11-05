namespace PhilSys.Features.GetLivenessKey;

public record GetLivenessKeyRequest() : ICommand<GetLivenessKeyResponse>;
public record GetLivenessKeyResponse(string LivenessKey);
public class GetLivenessKeyEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("philsys/getlivenesskey", (
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new GetLivenessKeyCommand();
			var result = sender.Send(command, cancellationToken);
			var response = result.Result.Adapt<GetLivenessKeyResponse>();
			return Results.Ok(response.LivenessKey);
		})
		  .WithName("GetLivenessKey")
		  .WithTags("PhilSys")
		  .Produces<GetLivenessKeyResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Get Liveness Key")
		  .WithDescription("Get Liveness Key for PhilSys verification");
	}
}
