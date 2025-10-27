
namespace Auth.Features.GetUserIdForForgotPassword;

public record GetUserIdForForgotPasswordRequest(GetUserIdForForgotPasswordRequestDTO GetUserIdForForgotPasswordRequestDTO) : ICommand<GetUserIdForForgotPasswordResponse>;

public record GetUserIdForForgotPasswordResponse(Guid UserId);

public class GetUserIdForForgotPasswordEndPoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("forgot-password/get-user-id", async (GetUserIdForForgotPasswordRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new GetUserIdForForgotPasswordCommand(request.GetUserIdForForgotPasswordRequestDTO);

			GetUserIdForForgotPasswordResult result = await sender.Send(command, cancellationToken);

			var response = new GetUserIdForForgotPasswordResponse(result.UserId);

			return Results.Ok(response);
		})
		  .WithName("GetUserIdForForgotPassword")
		  .WithTags("Authentication")
		  .Produces<GetUserIdForForgotPasswordResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Get User ID for Forgot Password")
		  .WithDescription("Retrieve the user ID associated with the provided email for password recovery.");
	}
}
