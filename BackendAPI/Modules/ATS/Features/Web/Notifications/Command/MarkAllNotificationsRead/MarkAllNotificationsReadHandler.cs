namespace ATS.Features.Web.Notifications.Command.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand : ICommand<MarkAllNotificationsReadResult>;

public record MarkAllNotificationsReadResult(int UpdatedCount);

public class MarkAllNotificationsReadHandler
	: ICommandHandler<MarkAllNotificationsReadCommand, MarkAllNotificationsReadResult>
{
	private readonly IAtsNotificationService _notificationService;
	private readonly ICurrentUser _currentUser;

	public MarkAllNotificationsReadHandler(
		IAtsNotificationService notificationService,
		ICurrentUser currentUser)
	{
		_notificationService = notificationService;
		_currentUser = currentUser;
	}

	public async Task<MarkAllNotificationsReadResult> Handle(
		MarkAllNotificationsReadCommand request,
		CancellationToken cancellationToken)
	{
		var recipientUserId = _currentUser.UserId;

		if (recipientUserId is null || recipientUserId == Guid.Empty)
		{
			return new MarkAllNotificationsReadResult(0);
		}

		var updatedCount = await _notificationService.MarkAllAsReadAsync(
			recipientUserId.Value,
			cancellationToken);

		return new MarkAllNotificationsReadResult(updatedCount);
	}
}
