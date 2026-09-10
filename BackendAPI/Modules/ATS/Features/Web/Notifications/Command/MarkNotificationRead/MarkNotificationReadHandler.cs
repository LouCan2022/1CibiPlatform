namespace ATS.Features.Web.Notifications.Command.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId)
	: ICommand<MarkNotificationReadResult>;

public record MarkNotificationReadResult(bool Success);

public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
	public MarkNotificationReadCommandValidator()
	{
		RuleFor(x => x.NotificationId)
			.NotEmpty().WithMessage("NotificationId is required.");
	}
}

public class MarkNotificationReadHandler
	: ICommandHandler<MarkNotificationReadCommand, MarkNotificationReadResult>
{
	private readonly IAtsNotificationService _notificationService;
	private readonly ICurrentUser _currentUser;

	public MarkNotificationReadHandler(
		IAtsNotificationService notificationService,
		ICurrentUser currentUser)
	{
		_notificationService = notificationService;
		_currentUser = currentUser;
	}

	public async Task<MarkNotificationReadResult> Handle(
		MarkNotificationReadCommand request,
		CancellationToken cancellationToken)
	{
		var recipientUserId = _currentUser.UserId;

		if (recipientUserId is null || recipientUserId == Guid.Empty)
		{
			return new MarkNotificationReadResult(false);
		}

		// The recipient is part of the UPDATE predicate, so another user's notification id
		// simply matches nothing rather than being fetched and rejected.
		var success = await _notificationService.MarkAsReadAsync(
			recipientUserId.Value,
			request.NotificationId,
			cancellationToken);

		return new MarkNotificationReadResult(success);
	}
}
