namespace ATS.Features.Web.Notifications.Query.GetNotifications;

public record GetNotificationsQueryRequest(
	string? Cursor = null,
	int? PageSize = 15,
	bool UnreadOnly = false)
	: IQuery<GetNotificationsQueryResult>;

public record GetNotificationsQueryResult(KeysetPaginatedResult<NotificationListDTO> Notifications);

public class GetNotificationsQueryRequestValidator : AbstractValidator<GetNotificationsQueryRequest>
{
	public GetNotificationsQueryRequestValidator()
	{
		RuleFor(x => x.PageSize)
			.Must(pageSize => pageSize is null || (pageSize > 0 && pageSize <= 100))
			.WithMessage("PageSize must be greater than 0 and less than or equal to 100.");

		// Cursor is deliberately unvalidated: cursors are opaque and a malformed one
		// self-heals to the first page rather than failing the request.
	}
}

public class GetNotificationsHandler
	: IQueryHandler<GetNotificationsQueryRequest, GetNotificationsQueryResult>
{
	private readonly IAtsNotificationService _notificationService;
	private readonly ICurrentUser _currentUser;

	public GetNotificationsHandler(
		IAtsNotificationService notificationService,
		ICurrentUser currentUser)
	{
		_notificationService = notificationService;
		_currentUser = currentUser;
	}

	public async Task<GetNotificationsQueryResult> Handle(
		GetNotificationsQueryRequest request,
		CancellationToken cancellationToken)
	{
		// The recipient comes from the validated token, never the request. This is the
		// whole isolation story for the inbox.
		var recipientUserId = _currentUser.UserId;

		if (recipientUserId is null || recipientUserId == Guid.Empty)
		{
			return new GetNotificationsQueryResult(
				new KeysetPaginatedResult<NotificationListDTO>(
					Array.Empty<NotificationListDTO>(),
					null,
					0));
		}

		var paginationRequest = new KeysetPaginationRequest(
			request.Cursor,
			request.PageSize ?? 15);

		var notifications = await _notificationService.GetNotificationsAsync(
			recipientUserId.Value,
			paginationRequest,
			request.UnreadOnly,
			cancellationToken);

		return new GetNotificationsQueryResult(notifications);
	}
}
