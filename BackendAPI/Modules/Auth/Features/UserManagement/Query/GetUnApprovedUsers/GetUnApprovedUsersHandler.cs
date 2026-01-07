namespace Auth.Features.UserManagement.Query.GetUnApprovedUsers;
public record GetUnApprovedUsersQueryRequest(
	int? PageNumber = 1, int?
	PageSize = 10,
	string? SearchTerm = null) : IQuery<GetUnApprovedUsersQueryResult>;

public record GetUnApprovedUsersQueryResult(PaginatedResult<UsersDTO> Users);

public class GetUnApprovedUsersQueryRequestValidator : AbstractValidator<GetUnApprovedUsersQueryRequest>
{
	public GetUnApprovedUsersQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}
public class GetUnApprovedUsersHandler : IQueryHandler<GetUnApprovedUsersQueryRequest, GetUnApprovedUsersQueryResult>
{
	private readonly IUserService _userService;
	public GetUnApprovedUsersHandler(IUserService userService)
	{
		_userService = userService;
	}
	public async Task<GetUnApprovedUsersQueryResult> Handle(
		GetUnApprovedUsersQueryRequest request,
		CancellationToken cancellationToken)
	{
		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);
		var userData = await _userService.GetUnApprovedUsersAsync(
			paginationRequest,
			cancellationToken);
		return new GetUnApprovedUsersQueryResult(userData);
	}
}
