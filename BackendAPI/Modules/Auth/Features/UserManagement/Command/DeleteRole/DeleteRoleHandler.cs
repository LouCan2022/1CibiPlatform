namespace Auth.Features.UserManagement.Command.DeleteRole;
public record DeleteRoleCommand(int RoleId) : ICommand<DeleteRoleResult>;
public record DeleteRoleResult(bool IsDeleted);
public class DeleteRoleHandler
{
    private readonly IRoleService _roleService;

    public DeleteRoleHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }
    public async Task<DeleteRoleResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var deletedRole = await _roleService.DeleteRoleAsync(request.RoleId);
        return new DeleteRoleResult(deletedRole);
    }
}



