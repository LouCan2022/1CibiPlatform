namespace Auth.Features.UserManagement.Command.DeleteAppSubRole;
public record DeleteAppSubRoleCommand(int AppSubRoleId) : ICommand<DeleteAppSubRoleResult>;
public record DeleteAppSubRoleResult(bool IsDeleted);
public class DeleteAppSubRoleHandler : ICommandHandler<DeleteAppSubRoleCommand, DeleteAppSubRoleResult>
{
	private readonly IAppSubRoleService _appSubRoleService;

	public DeleteAppSubRoleHandler(IAppSubRoleService appSubRoleService)
	{
		_appSubRoleService = appSubRoleService;
	}
	public async Task<DeleteAppSubRoleResult> Handle(DeleteAppSubRoleCommand request, CancellationToken cancellationToken)
	{
		var deletedAppSubRole = await _appSubRoleService.DeleteAppSubRoleAsync(request.AppSubRoleId);
		return new DeleteAppSubRoleResult(deletedAppSubRole);
	}
}


