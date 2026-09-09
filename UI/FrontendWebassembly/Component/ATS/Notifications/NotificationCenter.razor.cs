using FrontendWebassembly.ShareData.ATS;

namespace FrontendWebassembly.Component.ATS.Notifications;

public partial class NotificationCenter : IDisposable
{
	[Inject] private INotificationService Notifications { get; set; } = default!;
	[Inject] private NavigationManager Navigation { get; set; } = default!;
	[Inject] private ISnackbar Snackbar { get; set; } = default!;

	// Supplied by ATSLayout. Used to decide whether a notification's link is worth
	// rendering: deep-linking someone into a module they cannot open just bounces them
	// off /access-denied.
	[CascadingParameter(Name = "ATSAccessibleModuleIds")]
	private IReadOnlySet<int>? AccessibleModuleIds { get; set; }

	private const int PreviewCount = 8;

	private readonly List<NotificationDTO> _items = [];
	private bool _isOpen;
	private bool _isLoading;
	private bool _isBusy;

	private string BellAriaLabel => Notifications.UnreadCount switch
	{
		0 => "Notifications",
		1 => "Notifications, 1 unread",
		var count => $"Notifications, {count} unread"
	};

	// Caps at 99+: a three-digit count would push the pip out past the button's corner,
	// and the exact number stops being actionable long before then.
	private string UnreadLabel =>
		Notifications.UnreadCount > 99 ? "99+" : Notifications.UnreadCount.ToString();

	protected override async Task OnInitializedAsync()
	{
		Notifications.NotificationsChanged += OnNotificationsChanged;

		await Notifications.StartAsync();
	}

	// The hub event is a plain Action, so the boundary has to be void. The body lives in
	// an async Task so an exception cannot be thrown into the SignalR callback where
	// nothing is able to catch it - same reasoning as NewOrderComponent.OnATSResponse.
	private void OnNotificationsChanged(NotificationDTO? arrived) =>
		_ = OnNotificationsChangedAsync(arrived);

	private async Task OnNotificationsChangedAsync(NotificationDTO? arrived)
	{
		try
		{
			await InvokeAsync(() =>
			{
				if (arrived is not null)
				{
					// Keep the open dropdown honest without a refetch.
					_items.Insert(0, arrived);

					if (_items.Count > PreviewCount)
					{
						_items.RemoveRange(PreviewCount, _items.Count - PreviewCount);
					}

					if (ShouldToast(arrived.Type))
					{
						Snackbar.Add(arrived.Title, Severity.Info);
					}
				}

				StateHasChanged();
			});
		}
		catch (ObjectDisposedException)
		{
			// The component went away between the hub event and the render. Nothing to do.
		}
	}

	/// <summary>
	/// Whether an arriving notification also interrupts with a toast.
	/// </summary>
	/// <remarks>
	/// Everything still lands in the bell; this only decides what is loud enough to
	/// interrupt. The distinction is whether the event arrives one at a time or in a batch:
	///
	/// Ticketing and email failures are raised per order by background jobs, so a batch of
	/// 40 produced 40 toasts and buried the screen. They are the notifications most likely
	/// to arrive in bulk and the least likely to need acting on within the second, so the
	/// bell's count is the right weight for them.
	///
	/// The rest are one-per-event by nature - a candidate submits their own form, a bulk
	/// file finishes once - so a toast is proportionate.
	/// </remarks>
	private static bool ShouldToast(string type) => type switch
	{
		AtsNotificationTypes.TicketingFailed => false,
		AtsNotificationTypes.InvitationEmailFailed => false,
		_ => true
	};

	private async Task ToggleAsync()
	{
		_isOpen = !_isOpen;

		if (_isOpen)
		{
			await LoadPreviewAsync();
		}
	}

	private void Close() => _isOpen = false;

	private async Task LoadPreviewAsync()
	{
		_isLoading = true;

		var response = await Notifications.GetNotificationsAsync(pageSize: PreviewCount);

		_isLoading = false;

		if (!response.IsSuccess)
		{
			Snackbar.Add(response.ErrorDetail, Severity.Error);
			return;
		}

		_items.Clear();
		_items.AddRange(response.Data!.Items);
	}

	private async Task MarkAllReadAsync()
	{
		_isBusy = true;

		var response = await Notifications.MarkAllAsReadAsync();

		_isBusy = false;

		if (!response.IsSuccess)
		{
			Snackbar.Add(response.ErrorDetail, Severity.Error);
			return;
		}

		foreach (var item in _items)
		{
			item.IsRead = true;
		}
	}

	private async Task ActivateAsync(NotificationDTO item)
	{
		if (!item.IsRead)
		{
			// Marked read before navigating, so the badge is already correct by the time
			// the destination renders.
			var response = await Notifications.MarkAsReadAsync(item.NotificationId);

			if (response.IsSuccess)
			{
				item.IsRead = true;
			}
		}

		Close();

		if (string.IsNullOrWhiteSpace(item.LinkUrl))
		{
			return;
		}

		if (!CanOpen(item.LinkUrl))
		{
			// Read, but going nowhere: the notification is still worth seeing even when the
			// screen behind it is not the reader's to open.
			Snackbar.Add(
				"You do not have access to the screen this notification links to.",
				Severity.Info);

			return;
		}

		Navigation.NavigateTo(item.LinkUrl);
	}

	// Maps a link back to the ATS module that owns it, using ModuleList as the single
	// source of truth for route segments rather than a second hardcoded list that would
	// drift from it. Anything unrecognised is allowed: the destination page runs its own
	// RequireATSModule check, so this is a courtesy that avoids a pointless bounce, not
	// the access control itself.
	private bool CanOpen(string linkUrl)
	{
		if (AccessibleModuleIds is null || AccessibleModuleIds.Count == 0)
		{
			return true;
		}

		// Strip the query string before matching: "?search=Juan" must not be mistaken for
		// part of the route segment.
		var path = linkUrl.Split('?', '#')[0].TrimEnd('/');

		var module = ModuleList.List.FirstOrDefault(entry =>
			path.EndsWith($"/{entry.Value.path}", StringComparison.OrdinalIgnoreCase));

		return module.Key == 0 || AccessibleModuleIds.Contains(module.Key);
	}

	public void Dispose()
	{
		// Mandatory, not tidiness: the service is scoped, which in WASM means it lives as
		// long as the app. Without this every navigation would leave another subscription
		// behind and the user would get one duplicate toast per visit.
		Notifications.NotificationsChanged -= OnNotificationsChanged;
	}
}
