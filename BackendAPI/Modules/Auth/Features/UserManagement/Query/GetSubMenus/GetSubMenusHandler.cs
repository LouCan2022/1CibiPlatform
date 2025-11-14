namespace Auth.Features.UserManagement.Query.GetSubMenus;

public record GetSubMenusQueryRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null) : IQuery<GetSubMenusQueryResult>;

public record GetSubMenusQueryResult(PaginatedResult<SubMenusDTO> subMenus);

public class GetSubMenusQueryRequestValidator : AbstractValidator<GetSubMenusQueryRequest>
{
	public GetSubMenusQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}
public class GetSubMenusHandler : IQueryHandler<GetSubMenusQueryRequest, GetSubMenusQueryResult>
{
	private readonly ISubMenuService _subMenuService;

	public GetSubMenusHandler(ISubMenuService subMenuService)
	{
		_subMenuService = subMenuService;
	}
	public async Task<GetSubMenusQueryResult> Handle(GetSubMenusQueryRequest request, CancellationToken cancellationToken)
	{
		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);

		var subMenuData = await _subMenuService.GetSubMenusAsync(
			paginationRequest,
			cancellationToken);

		return new GetSubMenusQueryResult(subMenuData);
	}
}

