namespace FrontendWebassembly.Component.ATS;

public partial class ApplicationFormPreviewComponent
{
	[Parameter]
	public ApplicationFormPreviewDTO? Preview { get; set; }

	[CascadingParameter]
	private IMudDialogInstance MudDialog { get; set; } = default!;

	private void Close()
	{
		MudDialog.Cancel();
	}
}
