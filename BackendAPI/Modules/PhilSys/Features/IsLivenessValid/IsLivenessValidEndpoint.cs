
namespace PhilSys.Features.IsLivenessValid;

public record IsLivenessValidRequest(Guid Tid) : ICommand<IsLivenessValidResponse>;

public record IsLivenessValidResponse(TransactionStatusResponse TransactionStatusResponse);

public class IsLivenessValidEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("idv/validate/liveness", async (IsLivenessValidRequest request, ISender sender) =>
		{
			var command = new IsLivenessValidCommand(
				request.Tid
				);
			IsLivenessValidResult result = await sender.Send(command);
			var response = new IsLivenessValidResponse(result.TransactionStatusResponse);
			return Results.Ok(response.TransactionStatusResponse);
		})
		  .WithName("isLivenessVerified")
		  .WithTags("PhilSys")
		  .Produces<IsLivenessValidResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Liveness Session")
		  .WithDescription("Liveness Session");
	}
}
