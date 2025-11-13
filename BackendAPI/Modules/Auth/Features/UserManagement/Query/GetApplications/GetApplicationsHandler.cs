namespace Auth.Features.UserManagement.Query.GetApplications;

public record GetApplicationsQueryRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null) : IQuery<GetApplicationsQueryResult>;

public record GetApplicationsQueryResult(PaginatedResult<ApplicationsDTO> Applications);

public class GetApplicationsQueryRequestValidator : AbstractValidator<GetApplicationsQueryRequest>
{
	public GetApplicationsQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}
public class GetApplicationsHandler : IQueryHandler<GetApplicationsQueryRequest, GetApplicationsQueryResult>
{
	private readonly IApplicationService _applicationService;

	public GetApplicationsHandler(IApplicationService applicationService)
	{
		_applicationService = applicationService;
	}
	public async Task<GetApplicationsQueryResult> Handle(GetApplicationsQueryRequest request, CancellationToken cancellationToken)
	{

		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);

		var applicationData = await _applicationService.GetApplicationsAsync(
			paginationRequest,
			cancellationToken);

		return new GetApplicationsQueryResult(applicationData);
	}
}

