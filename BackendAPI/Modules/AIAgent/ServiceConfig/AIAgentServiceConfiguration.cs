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
		services.AddScoped<IPolicyRepository, PolicyRepository>();
		services.AddScoped<IExcelPolicyExtractorService, ExcelPolicyExtractorService>();
		services.AddScoped<IExcelQuestionExtractor, ExcelQuestionExtractor>();
		services.AddScoped<IFileStorageService, FileStorageService>();
		services.AddSignalR();
		return services;
	}

	#endregion

	#region Db Config

	public static IServiceCollection AddAIAgentInfrastructure(
	this IServiceCollection services,
	IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString(connStringSegment);

		var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
		dataSourceBuilder.UseVector();
		var dataSource = dataSourceBuilder.Build();

		services.AddDbContext<AIAgentApplicationDBContext>(options =>
		{
			options.UseNpgsql(
				dataSource,
				npgsqlOptions =>
				{
					npgsqlOptions.MigrationsAssembly(assemblyName);
					npgsqlOptions.UseVector();
				}
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
		var endpoint = configuration.GetValue<string>("OpenAI:Endpoint")!;
		var apiKey = configuration.GetValue<string>("OpenAI:ApiKey")!;
		var embeddingModel = configuration.GetValue<string>("OpenAI:EmbeddingModel")!;

		// Register Chat Client (LLM for conversation)
		services.AddOpenAIChatClient(
			modelId: configuration.GetValue<string>("OpenAI:Model")!,
			apiKey: apiKey,
			endpoint: new Uri(endpoint)
		);

		// ✅ Create HttpClient with Alibaba endpoint for embeddings
		var embeddingHttpClient = new HttpClient
		{
			BaseAddress = new Uri(endpoint)
		};

		// Register the embedding generator with custom HttpClient
#pragma warning disable SKEXP0010
		services.AddOpenAIEmbeddingGenerator(
			modelId: embeddingModel,
			apiKey: apiKey,
			dimensions: 1536,
			httpClient: embeddingHttpClient  // Pass HttpClient with Alibaba endpoint
		);
#pragma warning restore SKEXP0010

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
