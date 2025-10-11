namespace FrontendWebassembly.Component.Special;

using FrontendWebassembly.Services.Auth.Shared;
using Microsoft.AspNetCore.Components;
using System.Reflection;

public abstract class SecurePageBase : ComponentBase
{
	[Inject] protected NavigationManager Nav { get; set; } = default!;
	[Inject] protected IAccessService AccessService { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		var permissionAttr = GetType().GetCustomAttribute<RequirePermissionAttribute>();
		if (permissionAttr is null)
			return;

		bool hasAccess = await AccessService.HasAccessAsync(permissionAttr.AppId, permissionAttr.SubMenuId);
		if (!hasAccess)
		{
			Nav.NavigateTo("/access-denied");
		}
	}
}
