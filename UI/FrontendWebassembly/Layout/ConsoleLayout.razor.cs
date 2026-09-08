namespace FrontendWebassembly.Layout;

public partial class ConsoleLayout : IDisposable
{
	[Inject] public ThemeService Theme { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{
		await Theme.InitializeAsync();
		Theme.OnChanged += HandleThemeChanged;
	}

	private void HandleThemeChanged() => InvokeAsync(StateHasChanged);

	public void Dispose()
	{
		Theme.OnChanged -= HandleThemeChanged;
	}
}
