namespace FrontendWebassembly.Services.Auth.Shared;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class RequirePermissionAttribute : Attribute
{
	public int AppId { get; }
	public int SubMenuId { get; }

	public RequirePermissionAttribute(int appId, int subMenuId)
	{
		AppId = appId;
		SubMenuId = subMenuId;
	}
}