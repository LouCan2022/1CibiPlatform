namespace FrontendWebassembly.Layout;

public partial class SSOLayout
{
	private bool _drawerOpen = true;
	private bool _isLoading = true;

	// Mirrors ThemeService so the existing _isDarkMode-based style helpers below keep
	// working. Previously this layout owned the flag privately and never called
	// setStartupTheme, so toggling here left the <html> class and the Mud palette
	// disagreeing until the next full page load.
	private bool _isDarkMode => Theme.IsDarkMode;

	private const string _appIdKey = "AppId";
	private const string _subMenuKey = "SubMenuId";
	private const string _roleIdKey = "RoleId";

	private List<int> Apps = new List<int>();
	private List<List<int>> SubMenus = new List<List<int>>();
	private List<int> Roles = new List<int>();


	private static MudTheme _myTheme => ThemeService.Theme;

	protected override async Task OnInitializedAsync()
	{
		await Theme.InitializeAsync();
		Theme.OnChanged += HandleThemeChanged;
	}

	private void HandleThemeChanged() => InvokeAsync(StateHasChanged);

	public void Dispose() => Theme.OnChanged -= HandleThemeChanged;

	private void DrawerToggle() => _drawerOpen = !_drawerOpen;

	private async Task ToggleDarkMode() => await Theme.ToggleAsync();

	private string GetAppBarStyle()
	{
		return _isDarkMode
			? "background: linear-gradient(90deg, #68c0d6 0%, #2a77ae 50%, #102247 100%) !important;"
			: "background: linear-gradient(90deg, #102247 0%, #2a77ae 50%, #68c0d6 100%) !important;";
	}
	private string GetMenuIconStyle()
	{
		return _isDarkMode
			? "color: #102247;"
			: "color: white;";
	}

	private async Task LogoutAsync()
	{
		try
		{
			var success = await SSOService.LogoutAsync();
			await LocalStorageService.ClearAsync();
			await JS.InvokeVoidAsync("location.reload");

		}
		catch (Exception ex)
		{
		}
	}
}
