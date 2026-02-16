namespace Auth.Features.UserManagement.Query.GetLockedUsers;
public record GetLockedUsersQueryRequest(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null) : IQuery<GetLockedUsersQueryResult>;

public record GetLockedUsersQueryResult(PaginatedResult<AuthAttempts> LockedUsers);
public class GetLockedUsersQueryRequestValidator : AbstractValidator<GetLockedUsersQueryRequest>
{
	public GetLockedUsersQueryRequestValidator()
	{
		RuleFor(x => x.PageNumber).Must(pageIndex => pageIndex >= 0)
			.WithMessage("PageNumber must be greater than 0.");

		RuleFor(x => x.PageSize).Must(pageSize => pageSize > 0 && pageSize <= 100)
			.WithMessage("PageSize must be greater than 0.");
	}
}
public class GetLockedUsersHandler : IQueryHandler<GetLockedUsersQueryRequest, GetLockedUsersQueryResult>
{
	private readonly ILockerUserService _lockedUserService;
	public GetLockedUsersHandler(ILockerUserService lockedUserService)
	{
		_lockedUserService = lockedUserService;
	}
	public async Task<GetLockedUsersQueryResult> Handle(GetLockedUsersQueryRequest request, CancellationToken cancellationToken)
	{
		var paginationRequest = new PaginationRequest(
			request.PageNumber ?? 1,
			request.PageSize ?? 10,
			request.SearchTerm);

		var lockedUserData = await _lockedUserService.GetLockedUsersAsync(
			paginationRequest,
			cancellationToken);

		return new GetLockedUsersQueryResult(lockedUserData);
	}
}

