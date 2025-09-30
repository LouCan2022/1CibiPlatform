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


        #region Db Config

        public static IServiceCollection AddModuleCarter(
            this IServiceCollection services,
            Assembly assembly)
        {
            // Add Carter
            services.AddAuthCarterModules(assembly);
            return services;
        }

        #endregion

        public static IServiceCollection AddModuleServices(this IServiceCollection services)
        {
            services.AddTransient<IPasswordHasherService, PasswordHasherService>();
            services.AddTransient<InitialData>();
            //services.AddScoped<IJWTService, JWTService>();
            //services.AddScoped<ICacheService, MemoryCacheService>();
            //services.AddScoped<IPagination, Pagination>();

            ////For Add Credentials
            //services.AddScoped<IAddCredentialRepository, AddCredentialRepository>();

            ////For Get Credentials
            //services.AddScoped<IGetCredentialRepository, GetCredentialRepository>();
            //services.AddScoped<IPaginatedQ, GetCredentialRepository>();

            ////For Update Credentials
            //services.AddScoped<IUpdateCredentialRepository, UpdateCredentialRepository>();

            ////For Delete Credentials
            //services.AddScoped<IDeleteCredentialRepository, DeleteCredentialRepository>();

            //services.AddScoped<ILoginRepository, LoginRepository>();
            return services;
        }

    }
}
