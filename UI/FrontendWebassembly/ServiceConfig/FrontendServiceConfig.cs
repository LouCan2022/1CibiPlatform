namespace FrontendWebassembly.ServiceConfig;

public static class FrontendServiceConfig
{
	public static IServiceCollection AddFrontEndServices(this IServiceCollection services, IConfiguration configuration, Microsoft.AspNetCore.Components.WebAssembly.Hosting.IWebAssemblyHostEnvironment env)
	{
		// Allow configuration overrides
		var apiBaseFromConfig = configuration["ApiBase"];
		var ssoBaseFromConfig = configuration["SsoApiBase"];
		var prodDomainFromConfig = configuration["ProdDomain"];

		var apiBase = env.IsProduction()
			? (apiBaseFromConfig ?? prodDomainFromConfig)
			: apiBaseFromConfig;

		var ssoBase = env.IsProduction()
			? (ssoBaseFromConfig ?? prodDomainFromConfig)
			: ssoBaseFromConfig;

		services.AddHttpClient("API", client =>
		{
			client.BaseAddress = new Uri(apiBase);
		})
		 .AddHttpMessageHandler<CookieHandler>();

		services.AddHttpClient("SSOAPI", client =>
		{
			client.BaseAddress = new Uri(ssoBase);
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
