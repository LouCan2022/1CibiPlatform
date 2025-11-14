namespace FrontendWebassembly.ServiceConfig;

public static class FrontendServiceConfig
{
	public static IServiceCollection AddFrontEndServices(this IServiceCollection services)
	{
		services.AddHttpClient("API", client =>
		{
			client.BaseAddress = new Uri("http://localhost:4200");
		})
		 .AddHttpMessageHandler<CookieHandler>();

		services.AddHttpClient("SSOAPI", client =>
		{
			client.BaseAddress = new Uri("https://aurelio-baronetical-micki.ngrok-free.dev");
		})
		 .AddHttpMessageHandler<CookieHandler>();

		services.AddScoped<CookieHandler>();
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<LocalStorageService>();
		services.AddScoped<IAccessService, AccessService>();
		services.AddScoped<IPhilSysService, PhilSysService>();
		services.AddScoped<ISSOService, SSOService>();
		return services;
	}
}
