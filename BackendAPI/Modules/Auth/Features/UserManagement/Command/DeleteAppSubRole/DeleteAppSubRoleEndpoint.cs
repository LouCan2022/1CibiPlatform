using Auth.Features.UserManagement.Command.DeleteApplication;

namespace Auth.Features.UserManagement.Command.DeleteAppSubRole;

public record DeleteAppSubRoleRequest(int AppSubRoleId);
public record DeleteAppSubRoleResponse(bool IsDeleted);
public class DeleteAppSubRoleEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapDelete("auth/deleteappsubrole", async (DeleteAppSubRoleCommand request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new DeleteAppSubRoleCommand(
				request.AppSubRoleId
				);
			DeleteAppSubRoleResult result = await sender.Send(command, cancellationToken);
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


