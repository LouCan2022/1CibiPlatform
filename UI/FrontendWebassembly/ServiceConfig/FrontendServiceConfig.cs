

namespace FrontendWebassembly.ServiceConfig;

public static class FrontendServiceConfig
{
	public static IServiceCollection AddFrontEndCServices(this IServiceCollection services)
	{
		services.AddScoped(sp =>
		{
			var client = new HttpClient
			{
				BaseAddress = new Uri("http://localhost:5123")
			};
			return client;
		});

		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<LocalStorageService>();
		services.AddScoped<IAccessService, AccessService>();
		services.AddScoped<IPhilSysService, PhilSysService>();
		return services;
	}
}
