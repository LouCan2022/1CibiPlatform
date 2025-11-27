namespace Auth.Features.UserManagement.Query.GetApplications;

public record GetApplicationsEndpointRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null);

public record GetApplicationsEndpointResponse(PaginatedResult<ApplicationsDTO> Applications);
public class GetApplicationsEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("auth/getapplications", async (
			[AsParameters] GetApplicationsEndpointRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var query = new GetApplicationsQueryRequest(
				request.PageNumber,
				request.PageSize,
				request.SearchTerm);

			var applications = await sender.Send(query, cancellationToken);

			var result = new GetApplicationsEndpointResponse(applications.Applications);

			return Results.Ok(result);
		})
		.WithName("GetApplications")
		.WithTags("User Management")
		.Produces<GetApplicationsEndpointResponse>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Get Applications")
		.WithDescription("Retrieves a list of applications.")
	    .RequireAuthorization();
	}
}

