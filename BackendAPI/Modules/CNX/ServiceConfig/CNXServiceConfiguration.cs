namespace CNX.ServiceConfig;

public static class CNXServiceConfiguration
{
    private const string assemblyName = "APIs";
    private const string connStringSegment = "OnePlatform_Connection";

    #region Carter Config

    public static IServiceCollection AddCNXCarterModules(this IServiceCollection services, Assembly assembly)
    {
        services.AddCarter(configurator: c =>
        {
            var modules = assembly.GetTypes()
                .Where(t => typeof(ICarterModule).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
            c.WithModules(modules);
        });
        return services;
    }


    #endregion

    #region MediaTR Config
    public static IServiceCollection AddCNXMediaTR(this IServiceCollection services, Assembly assembly)
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
    public static IServiceCollection AddCNXServices(this IServiceCollection services)
    {
        services.AddHttpClient("Talkpush", client =>
        {
            client.BaseAddress = new Uri("https://concentrix-ph.talkpush.com/api/talkpush_services/");

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });
        services.AddScoped<GetCandidateService>();
        return services;
    }

    #endregion

    #region Db Config

    public static IServiceCollection AddCNXInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CNXApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString(connStringSegment),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(assemblyName)
            );
        });
        return services;
    }

    #endregion
}
