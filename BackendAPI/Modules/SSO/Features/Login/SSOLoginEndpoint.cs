namespace SSO.Features.Login;

public record SSOLoginRequest();

public class SSOLoginEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("sso/login", async (ISender sender) =>
		{

			var command = new SSOLoginCommand();
			await sender.Send(command);
			return Results.Empty;

		}).WithTags("SSO")
		  .WithName("SSOLogin")
		  .Produces(StatusCodes.Status403Forbidden)
		  .WithSummary("SSOLogin")
		  .WithDescription("SSOLogin");
	}
}
