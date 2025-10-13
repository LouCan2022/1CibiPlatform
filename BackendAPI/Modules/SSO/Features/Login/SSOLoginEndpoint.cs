namespace SSO.Features.Login;

public record SSOLoginRequest() : ICommand;

public class SSOLoginEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("sso/login", async (ISender sender) =>
		{

			var command = new SSOLoginRequest();
			await sender.Send(command);
			return Results.Ok();

		}).WithTags("SSO")
		  .WithName("SSOLogin")
		  .Produces(StatusCodes.Status403Forbidden)
		  .WithSummary("SSOLogin")
		  .WithDescription("SSOLogin");
	}
}
