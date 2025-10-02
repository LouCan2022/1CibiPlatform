using BuildingBlocks.Exceptions.Handler;

namespace Auth.ServiceConfig
{
    public static class AuthServiceConfiguration
    {
        private const string assemblyName = "APIs";
        private const string connStringSegment = "OnePlatform_Connection";


        #region Carter Config

        public static IServiceCollection AddAuthCarterModules(this IServiceCollection services, Assembly assembly)
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
        public static IServiceCollection AddAuthMediaTR(this IServiceCollection services, Assembly assembly)
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
        public static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddTransient<IPasswordHasherService, PasswordHasherService>();
            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            return services;
        }

        #endregion

        #region Db Config

        public static IServiceCollection AddAuthInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AuthApplicationDbContext>(options =>
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
}
