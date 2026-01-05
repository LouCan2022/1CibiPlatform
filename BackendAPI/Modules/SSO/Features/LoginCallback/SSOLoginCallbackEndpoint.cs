namespace SSO.Features.LoginCallback;

public record SSOLoginCallbackRequest(string? ReturnUrl = "/");

public record SSOLoginCallbackResponse(SSOLoginResponseDTO Result);


public class SSOLoginCallbackEndpoint : ICarterModule
{

	private readonly string _blazorAppUrl;
	private readonly IConfiguration _configuration;

	public SSOLoginCallbackEndpoint(IConfiguration configuration)
	{

		this._configuration = configuration;
		_blazorAppUrl = _configuration!.GetValue<string>("SSOMetadata:BlazorAppUrl")!;
	}

	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("sso/login/callback", async (string? returnUrl, ISender sender, HttpContext httpContext, CancellationToken cancellationToken) =>
		{
			// Use default if returnUrl is null or empty
			var safeReturnUrl = string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl;

			var command = new SSOLoginCallbackCommand(safeReturnUrl);

			SSOLoginCallbackResult result = await sender.Send(command, cancellationToken);

			return Results.Redirect(_blazorAppUrl);

		}).WithTags("SSO")
		  .WithName("SSOLoginCallback")
		  .Produces<SSOLoginCallbackResult>(StatusCodes.Status200OK)
		  .ProducesProblem(StatusCodes.Status403Forbidden)
		  .WithSummary("SSOLoginCallback")
		  .WithDescription("SSOLoginCallback");
	}
}
