namespace ATS.Features.Web.Notifications.Query.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQueryRequest
	: IQuery<GetUnreadNotificationCountQueryResult>;

public record GetUnreadNotificationCountQueryResult(NotificationUnreadCountDTO Count);

public class GetUnreadNotificationCountHandler
	: IQueryHandler<GetUnreadNotificationCountQueryRequest, GetUnreadNotificationCountQueryResult>
{
	private readonly IAtsNotificationService _notificationService;
	private readonly ICurrentUser _currentUser;

	public GetUnreadNotificationCountHandler(
		IAtsNotificationService notificationService,
		ICurrentUser currentUser)
	{
		_notificationService = notificationService;
		_currentUser = currentUser;
	}

	public async Task<GetUnreadNotificationCountQueryResult> Handle(
		GetUnreadNotificationCountQueryRequest request,
		CancellationToken cancellationToken)
	{
		var recipientUserId = _currentUser.UserId;

		if (recipientUserId is null || recipientUserId == Guid.Empty)
		{
			return new GetUnreadNotificationCountQueryResult(new NotificationUnreadCountDTO());
		}

		var count = await _notificationService.GetUnreadCountAsync(
			recipientUserId.Value,
			cancellationToken);

		return new GetUnreadNotificationCountQueryResult(count);
	}
}
