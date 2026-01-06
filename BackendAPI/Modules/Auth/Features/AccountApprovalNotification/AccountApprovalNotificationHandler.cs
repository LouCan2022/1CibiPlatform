namespace Auth.Features.AccountApprovalNotification;
public record AccountApprovalNotificationCommand(string Gmail) : ICommand<AccountApprovalNotificationResult>;
public record AccountApprovalNotificationResult(bool IsSent);
public class AccountApprovalNotificationHandler : ICommandHandler<AccountApprovalNotificationCommand, AccountApprovalNotificationResult>
{
	private readonly IUserService _userService;
	public AccountApprovalNotificationHandler(IUserService userService)
	{
		_userService = userService;
	}

	public async Task<AccountApprovalNotificationResult> Handle(AccountApprovalNotificationCommand request, CancellationToken cancellationToken)
	{

		var result = await _userService.SendApprovalToUserEmailAsync(request.Gmail);

		return new AccountApprovalNotificationResult(result);
	}

}
