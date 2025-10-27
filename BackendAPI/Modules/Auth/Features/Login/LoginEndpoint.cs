
namespace Auth.Features.Login;

public record LoginRequest(LoginCred LoginCred) : ICommand<LoginResponse>;

public record LoginResponse(LoginResponseDTO LoginResponseDTO);


public class LoginEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("login", async (LoginRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			LoginCred cred = request.LoginCred.Adapt<LoginCred>();

			var command = new LoginCommand(cred);

			LoginResult result = await sender.Send(command, cancellationToken);

			LoginResponse loginResponse = new LoginResponse(result.loginResponseDTO);

			return Results.Ok(loginResponse.LoginResponseDTO);
		})
		  .WithName("Login")
		  .WithTags("Authentication")
		  .Produces<LoginResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Login")
		  .WithDescription("Login User");
	}
}
