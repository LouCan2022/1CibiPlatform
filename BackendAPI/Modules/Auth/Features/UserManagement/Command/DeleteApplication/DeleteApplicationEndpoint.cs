namespace Auth.Features.UserManagement.Command.DeleteApplication;

public record DeleteApplicationRequest(int AppId);
public record DeleteApplicationResponse(bool IsDeleted);
public class DeleteApplicationEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("deleteapplication", async (DeleteApplicationCommand request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new DeleteApplicationCommand(
				request.AppId
				);
			DeleteApplicationResult result = await sender.Send(command, cancellationToken);
			var response = new DeleteApplicationResponse(result.IsDeleted);
			return Results.Ok(response.IsDeleted);
		})
		.WithName("DeleteApplication")
		.WithTags("User Management")
		.Produces<DeleteApplicationResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Delete Application")
		.WithDescription("Deletes an existing application in OnePlatform.");
	}
}
