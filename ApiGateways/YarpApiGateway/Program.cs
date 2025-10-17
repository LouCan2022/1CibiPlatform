using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
	.AddTransforms(builderContext =>
	{
		// Enable WebSocket support for Blazor debugging
		builderContext.AddRequestTransform(transformContext =>
		{
			var request = transformContext.HttpContext.Request;
			
			// Check if this is a WebSocket upgrade request
			if (request.Headers.ContainsKey("Upgrade") && 
			    request.Headers["Upgrade"].ToString().Contains("websocket", StringComparison.OrdinalIgnoreCase))
			{
				transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Connection", "Upgrade");
				transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Upgrade", "websocket");
			}

			return ValueTask.CompletedTask;
		});
	});

var app = builder.Build();

// Enable WebSockets middleware
app.UseWebSockets(new WebSocketOptions
{
	KeepAliveInterval = TimeSpan.FromMinutes(2)
});

app.MapReverseProxy();

app.Run();
