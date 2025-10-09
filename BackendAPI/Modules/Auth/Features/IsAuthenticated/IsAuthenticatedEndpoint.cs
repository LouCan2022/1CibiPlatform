namespace Auth.Features.IsAuthenticated;

public record IsAuthenticatedRequest() : ICommand<IsAuthenticatedResponse>;

public record IsAuthenticatedResponse(bool IsAuthenticated);

public class IsAuthenticatedEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("isAuthenticated", (
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new IsAuthenticatedCommand();

			var result = sender.Send(command, cancellationToken);

			var response = result.Result.Adapt<IsAuthenticatedResponse>();

			return Results.Ok(response);

		})
		  .WithName("IsAuthenticated")
		  .WithTags("Authentication")
		  .Produces<IsAuthenticatedResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("IsAuthenticated")
		  .WithDescription("IsAuthenticated");
	}
}
