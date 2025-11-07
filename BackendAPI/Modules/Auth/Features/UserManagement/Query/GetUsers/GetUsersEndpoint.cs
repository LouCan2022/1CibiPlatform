namespace Auth.Features.UserManagement.Query.GetUsers;

public record GetUsersEndpointRequest(int? PageNumber = 1, int? PageSize = 10);

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
			var command = new GetUsersQueryRequest(request.PageNumber, request.PageSize);

			var users = await sender.Send(command, cancellationToken);

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
