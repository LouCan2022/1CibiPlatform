namespace SSO.Features.IsUserAuthenticated;

public record IsUserAuthenticatedRequest() : ICommand<IsUserAuthenticatedResponse>;

public record IsUserAuthenticatedResponse(bool IsAuthenticated);


public class IsUserAuthenticatedEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("sso/is-user-authenticated", async (
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var command = new IsUserAuthenticatedCommand();

			IsUserAuthenticatedResult result = await sender.Send(command);

			IsUserAuthenticatedResponse response = new IsUserAuthenticatedResponse(result.IsAuthenticated);

			return Results.Ok(response);
		})
		  .WithName("IsUserAuthenticated")
		  .WithTags("SSO")
		  .Produces<IsUserAuthenticatedResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Check if user is authenticated")
		  .WithDescription("Checks if the current user is authenticated.");
	}
}
