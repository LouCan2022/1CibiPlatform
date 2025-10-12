namespace SSO.Features.LoginCallback;

public record SSOLoginCallbackRequest(string ReturnUrl = "/") : ICommand<SSOLoginResponseDTO>;

public record SSOLoginCallbackResponse(SSOLoginResponseDTO Result);


public class SSOLoginCallbackEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("/sso/login/callback", async (string returnUrl, ISender sender, HttpContext httpContext, CancellationToken cancellationToken) =>
		{
			var command = new SSOLoginCallbackRequest(returnUrl);

			SSOLoginResponseDTO result = await sender.Send(command, cancellationToken);

			var blazorAppUrl = $"https://localhost:7001/login-callback?&email={result.Email}&name={result.Name}";

			return Results.Redirect(blazorAppUrl);

		}).WithTags("SSO")
		  .WithName("SSOLoginCallback")
		  .Produces<SSOLoginResponseDTO>(StatusCodes.Status200OK)
		  .ProducesProblem(StatusCodes.Status403Forbidden)
		  .WithSummary("SSOLoginCallback")
		  .WithDescription("SSOLoginCallback");
	}
}
