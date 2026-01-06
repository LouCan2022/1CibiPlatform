namespace SSO.ServiceConfig;

public static class SSOServiceConfiguration
{
	#region MediaTR Config
	public static IServiceCollection AddSSOMediaTR(this IServiceCollection services, Assembly assembly)
	{
		// Add MediatR
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

	#region Services
	public static IServiceCollection AddSSOServices(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();
		return services;
	}

	#endregion
}
