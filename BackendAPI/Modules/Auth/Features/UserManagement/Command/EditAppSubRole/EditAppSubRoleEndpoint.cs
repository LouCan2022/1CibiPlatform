using Auth.Features.UserManagement.Command.EditSubMenu;

namespace Auth.Features.UserManagement.Command.EditAppSubRole;
public record EditAppSubRoleRequest(EditAppSubRoleDTO editAppSubRole);

public record EditAppSubRoleResponse(AppSubRoleDTO appSubRole);
public class EditAppSubRoleEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("editappsubrole", async (EditSubMenuCommand request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new EditSubMenuCommand(request.editSubMenu);
			EditSubMenuResult result = await sender.Send(command, cancellationToken);
			var response = new EditSubMenuResponse(result.subMenu);
			return Results.Ok(response.subMenu);
		})
		.WithName("EditAppSubRole")
		.WithTags("User Management")
		.Produces<EditAppSubRoleResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Edit AppSubRole")
		.WithDescription("Edits an application, submenu, and a role of a user in OnePlatform");
	}
}


