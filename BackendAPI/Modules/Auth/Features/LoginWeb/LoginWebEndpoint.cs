using static Auth.Features.LoginWeb.LoginWebHandler;

namespace Auth.Features.LoginWeb;

public record LoginWebRequest(LoginCred LoginCred) : ICommand<LoginWebResponse>;

public record LoginWebResponse(LoginResponseWebDTO loginResponseWebDTO);


public class LoginWebEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("Loginweb", async (LoginWebRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			LoginCred cred = request.LoginCred.Adapt<LoginCred>();

			var command = new LoginWebCommand(cred);

			LoginWebResult result = await sender.Send(command, cancellationToken);

			LoginWebResponse loginResponse = new LoginWebResponse(result.loginResponseWebDTO);

			return Results.Ok(loginResponse.loginResponseWebDTO);
		})
		  .WithName("Loginweb")
		  .WithTags("Authentication")
		  .Produces<LoginWebResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Login")
		  .WithDescription("Login");
	}
}

