namespace Auth.Features.UserManagement.Query.GetRoles;

public record GetRolesQueryRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null) : IQuery<GetRolesQueryResult>;

public record GetRolesQueryResult(PaginatedResult<RolesDTO> Roles);

public class GetRolesQueryRequestValidator : AbstractValidator<GetRolesQueryRequest>
{
	public GetRolesQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}
public class GetRolesHandler : IQueryHandler<GetRolesQueryRequest, GetRolesQueryResult>
{
	private readonly IRoleService _roleService;

	public GetRolesHandler(IRoleService roleService)
	{
		_roleService = roleService;
	}
	public async Task<GetRolesQueryResult> Handle(GetRolesQueryRequest request, CancellationToken cancellationToken)
	{
		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);

		var roleData = await _roleService.GetRolesAsync(
			paginationRequest,
			cancellationToken);

		return new GetRolesQueryResult(roleData);
	}
}


