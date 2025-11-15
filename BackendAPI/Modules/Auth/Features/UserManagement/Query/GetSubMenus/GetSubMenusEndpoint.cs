namespace Auth.Features.UserManagement.Query.GetSubMenus;

public record GetSubMenusEndpointRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null);
public record GetSubMenusEndpointResponse(PaginatedResult<SubMenusDTO> SubMenus);
public class GetSubMenusEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("auth/getsubmenus", async (
			[AsParameters] GetSubMenusEndpointRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var query = new GetSubMenusQueryRequest(
				request.PageNumber,
				request.PageSize,
				request.SearchTerm);

			var subMenus = await sender.Send(query, cancellationToken);

			var result = new GetSubMenusEndpointResponse(subMenus.subMenus);

			return Results.Ok(result);
		})
		.WithName("GetSubMenus")
		.WithTags("User Management")
		.Produces<GetSubMenusEndpointResponse>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Get SubMenus")
		.WithDescription("Retrieves a list of submenus.");
	}
}

