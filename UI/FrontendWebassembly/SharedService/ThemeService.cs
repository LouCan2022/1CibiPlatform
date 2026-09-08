namespace FrontendWebassembly.SharedService;

/// <summary>
/// Single owner of the dark-mode preference.
///
/// Before this existed, MainLayout and SSOLayout each kept their own _isDarkMode
/// field and their own MudTheme, and SSOLayout never told the &lt;html&gt; element about
/// a change - so toggling there left the startup class and the Mud palette
/// disagreeing. Everything now reads and writes through here.
///
/// The preference lives in localStorage under "isDarkMode" and is deliberately
/// preserved across logout (see AuthService.LogoutAsync and
/// MainLayout.OnInitializedAsync, both of which read it back after ClearAsync).
/// </summary>
public class ThemeService
{
	public const string StorageKey = "isDarkMode";

	private readonly LocalStorageService _localStorage;
	private readonly IJSRuntime _js;
	private bool _isInitialized;

	public ThemeService(LocalStorageService localStorage, IJSRuntime js)
	{
		_localStorage = localStorage;
		_js = js;
	}

	public bool IsDarkMode { get; private set; }

	/// <summary>
	/// Raised after the theme changes so every layout and topbar currently on
	/// screen can re-render. Subscribers must unsubscribe on dispose.
	/// </summary>
	public event Action? OnChanged;

	/// <summary>
	/// Reads the stored preference and syncs the &lt;html&gt; class. Safe to call from
	/// every layout: the read only happens once per session, so nested layouts
	/// (ATSLayout inside ConsoleLayout) do not each pay a JS interop round trip.
	/// </summary>
	public async Task InitializeAsync()
	{
		if (_isInitialized)
			return;

		_isInitialized = true;

		var stored = await _localStorage.GetItemAsync<bool?>(StorageKey);
		IsDarkMode = stored ?? false;

		await ApplyToDocumentAsync();
	}

	public async Task ToggleAsync() => await SetAsync(!IsDarkMode);

	public async Task SetAsync(bool isDarkMode)
	{
		if (_isInitialized && IsDarkMode == isDarkMode)
			return;

		_isInitialized = true;
		IsDarkMode = isDarkMode;

		await _localStorage.SetItemAsync(StorageKey, isDarkMode);
		await ApplyToDocumentAsync();

		OnChanged?.Invoke();
	}

	/// <summary>
	/// Mirrors the flag onto &lt;html&gt; so the token overrides in css/theme.css apply.
	/// index.html defines setStartupTheme and calls it before Blazor boots to avoid
	/// a flash of light content.
	/// </summary>
	private async Task ApplyToDocumentAsync()
	{
		try
		{
			await _js.InvokeVoidAsync("setStartupTheme", IsDarkMode);
		}
		catch (JSDisconnectedException)
		{
			// Circuit torn down mid-navigation; the startup script re-applies the
			// class from localStorage on the next load anyway.
		}
	}

	/// <summary>
	/// The one MudTheme for the whole app. Kept here rather than duplicated in each
	/// layout so the Mud palette and the CSS tokens in css/theme.css stay in step.
	/// </summary>
	public static MudTheme Theme { get; } = new()
	{
		PaletteLight = new PaletteLight
		{
			Primary = "#1d5fd1",
			Secondary = "#2f6fed",
			Background = "#f4f7fb",
			Surface = "#ffffff",
			AppbarBackground = "#0b1b3d",
			AppbarText = "#ffffff",
			DrawerBackground = "#ffffff",
			TextPrimary = "#16233f",
			TextSecondary = "#5b6b8c",
			Success = "#1e9e64",
			Warning = "#b5790f",
			Error = "#d64545",
			Info = "#1d5fd1",
			LinesDefault = "#d9e3f5",
			TableLines = "#e4eaf6"
		},
		PaletteDark = new PaletteDark
		{
			// Mirrors the html.dark block in css/theme.css. Change both together.
			Primary = "#6ea8ff",
			Secondary = "#7ea6ff",
			Background = "#0e131c",
			Surface = "#161c28",
			AppbarBackground = "#161c28",
			AppbarText = "#e8edf7",
			DrawerBackground = "#161c28",
			TextPrimary = "#e8edf7",
			TextSecondary = "#a3b0c8",
			Success = "#4ade9b",
			Warning = "#e0a94a",
			Error = "#ff7b7b",
			Info = "#6ea8ff",
			LinesDefault = "#2a3446",
			TableLines = "#222b3a",
			ActionDefault = "#a3b0c8",
			DrawerText = "#e8edf7",
			DrawerIcon = "#a3b0c8"
		},
		LayoutProperties = new LayoutProperties
		{
			DefaultBorderRadius = "10px",
			AppbarHeight = "64px"
		}
	};
}
