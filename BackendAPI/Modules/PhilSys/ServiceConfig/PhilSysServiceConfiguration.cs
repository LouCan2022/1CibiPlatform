namespace PhilSys.ServiceConfig;
public static class PhilSysServiceConfiguration
{
	#region MediatR Config
	public static IServiceCollection AddPhilSysMediaTR(this IServiceCollection services, Assembly assembly)
	{
		services.AddMediatR(config =>
		{
			config.RegisterServicesFromAssembly(assembly);
			config.AddOpenBehavior(typeof(ValidationBehavior<,>));
			config.AddOpenBehavior(typeof(LoggingBehavior<,>));
		});

		services.AddValidatorsFromAssembly(assembly);
		services.AddExceptionHandler<CustomExceptionHandler>();
		return services;
	}
	#endregion

	#region MediatR Config
	public static IServiceCollection AddPhilSysServices(this IServiceCollection services)
	{
		services.AddHttpClient("PhilSys", client =>
		{
			client.BaseAddress = new Uri("https://ws.everify.gov.ph/api/");

			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
		});
		services.AddScoped<GetTokenService>();
		return services;
	}
	#endregion
}