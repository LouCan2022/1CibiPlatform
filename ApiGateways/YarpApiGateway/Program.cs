var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
	options.AddPolicy("DevCors", policy =>
	{
		policy.WithOrigins("http://localhost:5134")
			  .AllowCredentials()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});


builder.Services.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseWebSockets();

app.MapReverseProxy();

app.UseCors("DevCors");

app.Run();
