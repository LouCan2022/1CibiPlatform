namespace FrontEnd.ServiceConfig;

public static class FrontendServiceConfig
{
    public static IServiceCollection AddFrontEndCServices(this IServiceCollection services)
    {
        services.AddHttpClient("OnePlatformAPI", client =>
        {
            client.BaseAddress = new Uri("http://apis:8080");

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IAuthRepository, AuthRepository>();
        return services;
    }
}
