namespace SSO.Features.LoginCallback;

public record SSOLoginCallbackRequest(string? ReturnUrl = "/");

public record SSOLoginCallbackResponse(SSOLoginResponseDTO Result);


public class SSOLoginCallbackEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("sso/login/callback", async (string? returnUrl, ISender sender, HttpContext httpContext, CancellationToken cancellationToken) =>
		{
			// Use default if returnUrl is null or empty
			var safeReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

			var command = new SSOLoginCallbackCommand(safeReturnUrl);

			SSOLoginCallbackResult result = await sender.Send(command, cancellationToken);

			var blazorAppUrl = $"http://localhost:5134/sso/frontpage";

			return Results.Redirect(blazorAppUrl);

		}).WithTags("SSO")
		  .WithName("SSOLoginCallback")
		  .Produces<SSOLoginCallbackResult>(StatusCodes.Status200OK)
		  .ProducesProblem(StatusCodes.Status403Forbidden)
		  .WithSummary("SSOLoginCallback")
		  .WithDescription("SSOLoginCallback");
	}
}
