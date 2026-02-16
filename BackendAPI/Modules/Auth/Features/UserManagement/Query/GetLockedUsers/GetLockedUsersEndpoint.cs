namespace Auth.Features.UserManagement.Query.GetLockedUsers;
public record GetLockedUsersEndpointRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null);

public record GetLockedUsersEndpointResponse(PaginatedResult<AuthAttempts> LockedUsers);
public class GetLockedUsersEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("auth/getlockedusers", async (
			[AsParameters] GetLockedUsersEndpointRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var query = new GetLockedUsersQueryRequest(
				request.PageNumber,
				request.PageSize,
				request.SearchTerm);

			var lockedUsers = await sender.Send(query, cancellationToken);

			var result = new GetLockedUsersEndpointResponse(lockedUsers.LockedUsers);

			return Results.Ok(result);
		})
		.WithName("GetLockedUsers")
		.WithTags("User Management")
		.Produces<GetLockedUsersEndpointResponse>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Get Locked Users")
		.WithDescription("Retrieves a list of locked users.")
		.RequireAuthorization();
	}
}

