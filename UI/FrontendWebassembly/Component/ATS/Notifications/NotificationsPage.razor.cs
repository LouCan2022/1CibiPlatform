using FrontendWebassembly.ShareData.ATS;

namespace FrontendWebassembly.Component.ATS.Notifications;

public partial class NotificationsPage : IDisposable
{
	[Inject] private IJSRuntime JS { get; set; } = default!;

	private const int PageSize = 20;

	private readonly List<NotificationDTO> _items = [];
	private ElementReference _sentinel;
	private DotNetObjectReference<NotificationsPage>? _selfReference;
	private IJSObjectReference? _observer;
	private string? _nextCursor;
	private bool _unreadOnly;
	private bool _isLoading;
	private bool _isLoadingMore;
	private bool _isBusy;

	protected override async Task OnInitializedAsync()
	{
		// SecurePageBase runs the permission and module checks.
		await base.OnInitializedAsync();

		if (!IsPageAuthorized)
		{
			return;
		}

		Notifications.NotificationsChanged += OnNotificationsChanged;

		// The layout normally has the connection open already; this covers a deep link
		// straight onto this page.
		await Notifications.StartAsync();

		await LoadFirstPageAsync();
	}

	// The sentinel only exists once a page has rendered and a next cursor is present, so
	// the observer is attached after each render rather than once on first render.
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (_nextCursor is null || _observer is not null || _items.Count == 0)
		{
			return;
		}

		_selfReference ??= DotNetObjectReference.Create(this);

		_observer = await JS.InvokeAsync<IJSObjectReference>(
			"notificationScroll.observe",
			_sentinel,
			_selfReference);
	}

	/// <summary>
	/// Called from JS when the sentinel scrolls into view.
	/// </summary>
	[JSInvokable]
	public async Task OnSentinelVisibleAsync() => await LoadMoreAsync();

	private string FilterCssClass(bool unreadOnly) =>
		_unreadOnly == unreadOnly
			? "ats-segment-btn active"
			: "ats-segment-btn";

	private async Task SetFilterAsync(bool unreadOnly)
	{
		if (_unreadOnly == unreadOnly)
		{
			return;
		}

		_unreadOnly = unreadOnly;

		await LoadFirstPageAsync();
	}

	private async Task LoadFirstPageAsync()
	{
		_isLoading = true;

		// A filter change starts a new keyset walk, so the observer is torn down with the
		// sentinel it was watching and re-attached against the new list.
		await DisposeObserverAsync();

		var response = await Notifications.GetNotificationsAsync(
			cursor: null,
			pageSize: PageSize,
			unreadOnly: _unreadOnly);

		_isLoading = false;

		if (!response.IsSuccess)
		{
			Snackbar.Add(response.ErrorDetail, Severity.Error);
			return;
		}

		_items.Clear();
		_items.AddRange(response.Data!.Items);
		_nextCursor = response.Data.NextCursor;
	}

	private async Task LoadMoreAsync()
	{
		// Guarded because the observer can fire again while a page is still in flight.
		if (_isLoadingMore || _nextCursor is null)
		{
			return;
		}

		_isLoadingMore = true;

		var response = await Notifications.GetNotificationsAsync(
			cursor: _nextCursor,
			pageSize: PageSize,
			unreadOnly: _unreadOnly);

		if (!response.IsSuccess)
		{
			_isLoadingMore = false;
			Snackbar.Add(response.ErrorDetail, Severity.Error);
			return;
		}

		_items.AddRange(response.Data!.Items);
		_nextCursor = response.Data.NextCursor;

		// The sentinel moved with the new rows, so the old observation is stale.
		await DisposeObserverAsync();

		_isLoadingMore = false;

		await InvokeAsync(StateHasChanged);
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

		if (_unreadOnly)
		{
			// Nothing shown still matches the filter.
			await LoadFirstPageAsync();
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
			var response = await Notifications.MarkAsReadAsync(item.NotificationId);

			if (response.IsSuccess)
			{
				item.IsRead = true;
			}
		}

		if (string.IsNullOrWhiteSpace(item.LinkUrl))
		{
			return;
		}

		if (!CanOpen(item.LinkUrl))
		{
			Snackbar.Add(
				"You do not have access to the screen this notification links to.",
				Severity.Info);

			return;
		}

		Navigation.NavigateTo(item.LinkUrl);
	}

	// Same courtesy check as the bell: ModuleList owns the route segments, and the
	// destination page still runs its own RequireATSModule check.
	private bool CanOpen(string linkUrl)
	{
		if (AccessibleATSModuleIds.Count == 0)
		{
			return true;
		}

		var path = linkUrl.Split('?', '#')[0].TrimEnd('/');

		var module = ModuleList.List.FirstOrDefault(entry =>
			path.EndsWith($"/{entry.Value.path}", StringComparison.OrdinalIgnoreCase));

		return module.Key == 0 || AccessibleATSModuleIds.Contains(module.Key);
	}

	private void OnNotificationsChanged(NotificationDTO? arrived) =>
		_ = OnNotificationsChangedAsync(arrived);

	private async Task OnNotificationsChangedAsync(NotificationDTO? arrived)
	{
		try
		{
			await InvokeAsync(() =>
			{
				// A new arrival belongs at the top of the feed the reader is already
				// looking at; refetching would lose their scroll position.
				if (arrived is not null)
				{
					_items.Insert(0, arrived);
				}

				StateHasChanged();
			});
		}
		catch (ObjectDisposedException)
		{
			// Navigated away between the hub event and the render.
		}
	}

	private async ValueTask DisposeObserverAsync()
	{
		if (_observer is null)
		{
			return;
		}

		var observer = _observer;

		// Cleared first: disposal runs during navigation and can yield, and a second
		// caller must not find a handle that is already being torn down.
		_observer = null;

		// The handle owns the IntersectionObserver; dispose() disconnects it.
		await SafeJs.InvokeVoidAsync(observer, "dispose");

		await SafeJs.DisposeAsync(observer);
	}

	public void Dispose()
	{
		// The service outlives the page, so this is mandatory - see NotificationCenter.
		Notifications.NotificationsChanged -= OnNotificationsChanged;

		_ = DisposeObserverAsync();

		_selfReference?.Dispose();
	}
}
