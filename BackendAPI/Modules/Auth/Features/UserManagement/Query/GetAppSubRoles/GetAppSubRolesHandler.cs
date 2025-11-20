namespace Auth.Features.UserManagement.Query.GetAppSubRoles;

public record GetAppSubRolesQueryRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null) : IQuery<GetAppSubRolesQueryResult>;

public record GetAppSubRolesQueryResult(PaginatedResult<AppSubRolesDTO> AppSubRoles);

public class GetAppSubRolesQueryRequestValidator : AbstractValidator<GetAppSubRolesQueryRequest>
{
	public GetAppSubRolesQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}
public class GetAppSubRolesHandler : IQueryHandler<GetAppSubRolesQueryRequest, GetAppSubRolesQueryResult>
{
	private readonly IAppSubRoleService _appSubRoleService;

	public GetAppSubRolesHandler(IAppSubRoleService appSubRoleService)
	{
		_appSubRoleService = appSubRoleService;
	}
	public async Task<GetAppSubRolesQueryResult> Handle(GetAppSubRolesQueryRequest request, CancellationToken cancellationToken)
	{
		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);

		var appSubRoleData = await _appSubRoleService.GetAppSubRolesAsync(
			paginationRequest,
			cancellationToken);

		return new GetAppSubRolesQueryResult(appSubRoleData);
	}
}


