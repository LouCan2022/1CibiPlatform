using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddReverseProxy()
//	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));


builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
	.AddTransforms(builderContext =>
	{
		builderContext.AddRequestTransform(transformContext =>
		{
			var path = transformContext.HttpContext.Request.Path;

			if (path.StartsWithSegments("/_framework/debug"))
			{
				transformContext.ProxyRequest.Headers.Add("Connection", "Upgrade");
				transformContext.ProxyRequest.Headers.Add("Upgrade", "websocket");
			}

			return ValueTask.CompletedTask;
		});
	});



var app = builder.Build();

app.UseWebSockets();

app.MapReverseProxy();

app.Run();
