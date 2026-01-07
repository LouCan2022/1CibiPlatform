namespace Auth.Features.UserManagement.Command.EditUser;
public record EditUserCommand(EditUserDTO editUser) : ICommand<EditUserResult>;
public record EditUserResult(UserDTO user);
public class EditUserHandler : ICommandHandler<EditUserCommand, EditUserResult>
{
	private readonly IUserService _userService;

	public EditUserHandler(IUserService userService)
	{
		_userService = userService;
	}
	public async Task<EditUserResult> Handle(EditUserCommand request, CancellationToken cancellationToken)
	{
		var editedUser = await _userService.EditUserAsync(request.editUser);
		return new EditUserResult(editedUser);
	}
}
