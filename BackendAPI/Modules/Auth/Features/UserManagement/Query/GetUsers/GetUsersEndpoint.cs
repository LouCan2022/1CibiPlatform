namespace Auth.Features.UserManagement.Query.GetUsers;

public record GetUsersEndpointRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null);

public record GetUsersEndpointResponse(PaginatedResult<UsersDTO> Users);

public class GetUsersEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("auth/getusers", async (
			[AsParameters] GetUsersEndpointRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var query = new GetUsersQueryRequest(
				request.PageNumber,
				request.PageSize,
				request.SearchTerm);

			var users = await sender.Send(query, cancellationToken);

			var result = new GetUsersEndpointResponse(users.Users);

			return Results.Ok(result);
		})
		.WithName("GetUsers")
		.WithTags("User Management")
		.Produces<GetUsersEndpointResponse>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Get Users")
		.WithDescription("Retrieves a list of users.");
	}
}
