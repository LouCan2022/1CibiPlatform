namespace Auth.Features.UserManagement.Query.GetUsers;

public record GetUsersQueryRequest(
	int? PageNumber = 1, int?
	PageSize = 10,
	string? SearchTerm = null) : IQuery<GetUsersQueryResult>;

public record GetUsersQueryResult(PaginatedResult<UsersDTO> Users);

public class GetUsersQueryRequestValidator : AbstractValidator<GetUsersQueryRequest>
{
	public GetUsersQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}

public class GetUsersQueryHandler : IQueryHandler<GetUsersQueryRequest, GetUsersQueryResult>
{
	private readonly IUserService _userService;

	public GetUsersQueryHandler(IUserService userService)
	{
		this._userService = userService;
	}

	public async Task<GetUsersQueryResult> Handle(
		GetUsersQueryRequest request,
		CancellationToken cancellationToken)
	{

		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);

		var userData = await this._userService.GetUsersAsync(
			paginationRequest,
			cancellationToken);

		return new GetUsersQueryResult(userData);
	}
}
