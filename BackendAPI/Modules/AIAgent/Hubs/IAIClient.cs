namespace AIAgent.Hubs;

public interface IAIClient
{
	Task ReceiveAiResponse(string message);
	Task ReceiveTyping(bool isTyping);
	Task SessionCleared();
}

public class AIAgentHub : Hub<IAIClient>
{
	public override Task OnConnectedAsync()
	{
		var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
		if (!string.IsNullOrEmpty(userId))
		{
			Groups.AddToGroupAsync(Context.ConnectionId, userId);
		}

		return base.OnConnectedAsync();
	}

	public override Task OnDisconnectedAsync(Exception? exception)
	{
		var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
		if (!string.IsNullOrEmpty(userId))
		{
			Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
		}

		return base.OnDisconnectedAsync(exception);
	}
}