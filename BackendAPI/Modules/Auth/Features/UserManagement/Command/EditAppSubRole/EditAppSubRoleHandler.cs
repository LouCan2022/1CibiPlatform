namespace Auth.Features.UserManagement.Command.EditAppSubRole;
public record EditAppSubRoleCommand(EditAppSubRoleDTO editAppSubRole) : ICommand<EditAppSubRoleResult>;

public record EditAppSubRoleResult(AppSubRoleDTO appSubRole);

public class EditAppSubRoleHandler : ICommandHandler<EditAppSubRoleCommand, EditAppSubRoleResult>
{
	private readonly IAppSubRoleService _appSubRoleService;

	public EditAppSubRoleHandler(IAppSubRoleService appSubRoleService)
	{
		_appSubRoleService = appSubRoleService;
	}
	public async Task<EditAppSubRoleResult> Handle(EditAppSubRoleCommand request, CancellationToken cancellationToken)
	{
		var editAppSubRole = await _appSubRoleService.EditAppSubRoleAsync(request.editAppSubRole);
		return new EditAppSubRoleResult(editAppSubRole);
	}
}

