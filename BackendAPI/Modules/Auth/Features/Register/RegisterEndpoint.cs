namespace Auth.Features.Register;

public record RegisterRequest(RegisterRequestDTO register) : ICommand<RegisterResponse>;

public record RegisterResponse(OtpVerificationResponse otpVerificationResponse);

public class RegisterEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("register", async (RegisterRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new RegisterRequestCommand(request.register);

			RegisterResult result = await sender.Send(command, cancellationToken);

			RegisterResponse response = new RegisterResponse(result.otpVerificationResponse);

			return Results.Ok(response);
		})
		  .WithName("register")
		  .WithTags("Authentication")
		  .Produces<RegisterResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("register")
		  .WithDescription("register");
	}
}
