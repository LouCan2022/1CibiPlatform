using Auth;

namespace APIs.ServiceConfig
{
    public static class ServiceConfiguration
    {

        #region JWT Config
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // JWT Authentication
            var jwtSettings = configuration.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                    RoleClaimType = ClaimTypes.Role,
                };
            });
            services.AddAuthorization();

            return services;
        }
        #endregion

        #region Db Config

        public static IServiceCollection AddModuleInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add DbContext
            services.AddAuthInfrastructure(configuration);
            return services;
        }

        #endregion

        #region Carter Config

        public static IServiceCollection AddModuleCarter(
            this IServiceCollection services)
        {
            var assembly = typeof(AuthMarker).Assembly;

            // Add Carter
            services.AddAuthCarterModules(assembly);
            return services;
        }

        #endregion

        #region MediaTR Config

        public static IServiceCollection AddModuleMediaTR(
            this IServiceCollection services)
        {
            // Add MediaTR
            var assembly = typeof(AuthMarker).Assembly;

            services.AddAuthMediaTR(assembly);

            return services;
        }

        #endregion

        #region Services Config
        public static IServiceCollection AddModuleServices(this IServiceCollection services)
        {
            // Add Services
            services.AddAuthServices();
            services.AddTransient<InitialData>();

            return services;
        }

        #endregion

    }
}
