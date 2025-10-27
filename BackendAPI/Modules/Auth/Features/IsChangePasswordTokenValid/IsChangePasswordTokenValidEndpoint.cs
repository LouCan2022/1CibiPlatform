namespace Auth.Features.IsChangePasswordTokenValid;

public record IsChangePasswordTokenValidRequest(ForgotPasswordTokenRequestDTO ForgotPasswordTokenRequestDTO) : ICommand<IsChangePasswordTokenValidResponse>;

public record IsChangePasswordTokenValidResponse(bool IsValid);

public class IsChangePasswordTokenValidEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("/is-change-password-token-valid",
			async (IsChangePasswordTokenValidRequest request,
				  ISender sender,
				  CancellationToken cancellationToken) =>
			{
				var command = new IsChangePasswordTokenValidCommand(request.ForgotPasswordTokenRequestDTO);

				IsChangePasswordTokenValidResult resultFromHandler = await sender.Send(command);

				var result = new IsChangePasswordTokenValidResponse(resultFromHandler.IsValid);

				return Results.Ok(result);
			})
			.WithName("IsChangePasswordTokenValid")
			.WithTags("Authentication")
			.Produces<IsChangePasswordTokenValidResponse>(StatusCodes.Status200OK)
			.WithSummary("Checks if the change password token is valid.")
			.WithDescription("Checks if the change password token is valid.");
	}
}
