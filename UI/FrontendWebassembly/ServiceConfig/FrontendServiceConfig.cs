
namespace FrontendWebassembly.ServiceConfig;

public static class FrontendServiceConfig
{
	public static IServiceCollection AddFrontEndServices(this IServiceCollection services)
	{
		services.AddHttpClient("API", client =>
		{
			client.BaseAddress = new Uri("http://localhost:5123");
		})
		 .AddHttpMessageHandler<CookieHandler>();

		services.AddScoped<CookieHandler>();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<LocalStorageService>();
		services.AddScoped<IAccessService, AccessService>();
		services.AddScoped<IPhilSysService, PhilSysService>();
		return services;
	}
}
