namespace SSO.Features.Logout;

public record SSOLogoutRequest() : ICommand;

public class SSOLogoutEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("sso/logout", async (SSOLogoutRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new SSOLogoutCommand();

			await sender.Send(command, cancellationToken);

			return Results.Redirect("https://localhost:7001/");
		})
		  .WithName("SSO Logout")
		  .WithTags("SSO")
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("SSO Logout")
		  .WithDescription("Logs out the user from the SSO session");
	}
}
