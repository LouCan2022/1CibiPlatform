namespace AIAgent.ServiceConfig;


public static class AIAgentServiceConfiguration
{
	private const string assemblyName = "APIs";
	private const string connStringSegment = "OnePlatform_Connection";

	#region MediaTR Config
	public static IServiceCollection AddAIAgentMediaTR(this IServiceCollection services, Assembly assembly)
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
	public static IServiceCollection AddAIAgentServices(this IServiceCollection services)
	{
		services.AddSingleton<IAIAgentService, AIAgentService>();
		services.AddSignalR();
		return services;
	}

	#endregion

	#region Db Config

	public static IServiceCollection AddAIAgentInfrastructure(
	this IServiceCollection services,
	IConfiguration configuration)
	{
		services.AddDbContext<AIAgentApplicationDBContext>(options =>
		{
			options.UseNpgsql(
				configuration.GetConnectionString(connStringSegment),
				npgsqlOptions => npgsqlOptions.MigrationsAssembly(assemblyName)
			);
		});
		return services;
	}

	#endregion

	#region OpenAI Config
	public static IServiceCollection AddAIAgentConfiguration(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddOpenAIChatClient
			(
			  modelId: configuration.GetValue<string>("OpenAI:Model")!,
			  apiKey: configuration.GetValue<string>("OpenAI:ApiKey")!,
			  endpoint: configuration.GetValue<Uri>("OpenAI:Endpoint")!
			);

		services.AddKernel();

		return services;
	}
	#endregion

	#region AI Agent Skills Config
	public static IServiceCollection AddAIAgentSkills(this IServiceCollection services, IConfiguration configuration)
	{
		// Register the registry; the startup filter will scan and import into kernel if available
		services.AddSingleton<SkillRegistry>();

		return services;
	}
	#endregion

}
