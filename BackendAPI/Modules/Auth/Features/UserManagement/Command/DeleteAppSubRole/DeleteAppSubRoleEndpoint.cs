using Auth.Features.UserManagement.Command.DeleteApplication;

namespace Auth.Features.UserManagement.Command.DeleteAppSubRole;

public record DeleteAppSubRoleRequest(int AppSubRoleId);
public record DeleteAppSubRoleResponse(bool IsDeleted);
public class DeleteAppSubRoleEndpoint : ICarterModule
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
		.WithName("DeleteAppSubRole")
		.WithTags("User Management")
		.Produces<DeleteAppSubRoleResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Delete AppSubRole")
		.WithDescription("Delete an application, submenu, and a role of a user in OnePlatform");
	}
}


