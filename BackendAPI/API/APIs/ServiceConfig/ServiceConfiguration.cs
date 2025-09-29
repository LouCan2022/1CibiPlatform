namespace APIs.ServiceConfig
{
    public static class ServiceConfiguration
    {

        #region Db Config

        public static IServiceCollection AddModuleInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddAuthInfrastructure(configuration);
            return services;
        }

        #endregion
    }
}
