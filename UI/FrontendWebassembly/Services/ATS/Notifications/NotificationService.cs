namespace FrontendWebassembly.Services.ATS.Notifications;

public class NotificationService : INotificationService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<NotificationService> _logger;
	private HubConnection? _hubConnection;

	public NotificationService(
		IHttpClientFactory httpClientFactory,
		ILogger<NotificationService> logger)
	{
		_httpClient = httpClientFactory.CreateClient("API");
		_logger = logger;
	}

	public long UnreadCount { get; private set; }

	public event Action<NotificationDTO?>? NotificationsChanged;

	public async Task StartAsync()
	{
		// Seed the badge first so it is right even if the hub never connects - the rows
		// are already in the database either way.
		await RefreshUnreadCountAsync();

		if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
		{
			return;
		}

		var baseUri = _httpClient.BaseAddress?.ToString()?.TrimEnd('/') ?? string.Empty;
		var hubUrl = $"{baseUri}/hubs/atsbulk";

		_hubConnection = new HubConnectionBuilder()
			.WithUrl(hubUrl, options =>
			{
				// Without this the auth cookie is not sent on the handshake, ATSHub's
				// Context.User is null, GetUserGroupName returns null, and the connection
				// joins no group - so nothing is ever delivered. It works in deployed
				// environments only because the gateway serves the UI and the API from one
				// origin; in local development they are different ports and the cookie is
				// dropped. CookieHandler sets Include + Cors, which is what the handshake
				// needs.
				options.HttpMessageHandlerFactory = innerHandler =>
					new CookieHandler { InnerHandler = innerHandler };
			})
			.WithAutomaticReconnect()
			.Build();

		_hubConnection.On<NotificationDTO>("ReceiveNotification", notification =>
		{
			UnreadCount++;
			NotificationsChanged?.Invoke(notification);
		});

		// A reconnect means the badge may have moved while the socket was down, so it is
		// re-read rather than assumed.
		_hubConnection.Reconnected += async _ => await RefreshUnreadCountAsync();

		_hubConnection.Closed += async ex =>
		{
			_logger.LogWarning(ex, "Notification hub connection closed.");
			await Task.CompletedTask;
		};

		// The bell still works without a live socket: the count is seeded above and every
		// page load re-reads it, so a failed connection must not break the layout.
		await StartHubSafelyAsync();
	}

	public Task<ServiceResponse<KeysetPaginatedResult<NotificationDTO>>> GetNotificationsAsync(
		string? cursor = null,
		int pageSize = 15,
		bool unreadOnly = false)
	{
		var query = $"ats/getnotifications?pageSize={pageSize}&unreadOnly={unreadOnly}";

		if (!string.IsNullOrEmpty(cursor))
			query += $"&cursor={Uri.EscapeDataString(cursor)}";

		return ApiRequestExtensions.SendAsync<GetNotificationsResponseDTO, KeysetPaginatedResult<NotificationDTO>>(
			() => _httpClient.GetAsync(query),
			result => result.Notifications);
	}

	public async Task<ServiceResponse<long>> RefreshUnreadCountAsync()
	{
		var response = await ApiRequestExtensions.SendAsync<GetUnreadNotificationCountResponseDTO, NotificationUnreadCountDTO>(
			() => _httpClient.GetAsync("ats/getunreadnotificationcount"),
			result => result.Count);

		if (!response.IsSuccess)
		{
			return ServiceResponse<long>.Failure(response.ErrorDetail);
		}

		UnreadCount = response.Data!.UnreadCount;
		NotificationsChanged?.Invoke(null);

		return ServiceResponse<long>.Success(UnreadCount);
	}

	public async Task<ServiceResponse<bool>> MarkAsReadAsync(Guid notificationId)
	{
		var response = await ApiRequestExtensions.SendAsync<bool>(
			() => _httpClient.PatchAsJsonAsync("ats/marknotificationread", new { notificationId }));

		if (response.IsSuccess && response.Data && UnreadCount > 0)
		{
			// Adjusted locally rather than re-read: the caller is usually navigating away,
			// and a round trip just to decrement by one is wasted.
			UnreadCount--;
			NotificationsChanged?.Invoke(null);
		}

		return response;
	}

	public async Task<ServiceResponse<int>> MarkAllAsReadAsync()
	{
		var response = await ApiRequestExtensions.SendAsync<int>(
			() => _httpClient.PatchAsJsonAsync("ats/markallnotificationsread", new { }));

		if (response.IsSuccess)
		{
			UnreadCount = 0;
			NotificationsChanged?.Invoke(null);
		}

		return response;
	}

	// SignalR's StartAsync throws on an unreachable hub rather than returning a status, and
	// there is no ServiceResponse-shaped equivalent to route it through. Kept to this one
	// method so the rest of the service stays free of transport handling.
	private async Task StartHubSafelyAsync()
	{
		try
		{
			await _hubConnection!.StartAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Could not open the notification hub connection.");
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_hubConnection is not null)
		{
			await _hubConnection.DisposeAsync();
			_hubConnection = null;
		}

		GC.SuppressFinalize(this);
	}
}
