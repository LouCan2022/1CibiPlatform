namespace Auth.ServiceConfig
{
    public static class AuthServiceConfiguration
    {
        private const string assemblyName = "APIs";
        private const string connStringSegment = "OnePlatform_Connection";

        #region CORS
        public static void ConfigureAuthCorsProd(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                                 policy =>
                                 {
                                     policy.WithOrigins(
                                         "http://192.168.32.21:4200",
                                         "http://localhost:5055")
                                           .AllowAnyHeader()
                                           .AllowAnyMethod()
                                           .AllowCredentials();
                                 });
            });
        }

        public static void ConfigureAuthCorsDev(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                                 policy =>
                                 {
                                     policy.WithOrigins(
                                         "http://localhost:4200",
                                         "http://localhost:5055")
                                           .AllowAnyHeader()
                                           .AllowAnyMethod()
                                           .AllowCredentials();
                                 });
            });
        #endregion

        #region JWT Config
        public static IServiceCollection AddAuthJwtAuthentication(
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

        #region Carter Config

        public static IServiceCollection AddAuthCarterModules(this IServiceCollection services, Assembly assembly)
        {
            services.AddCarter(configurator: c =>
            {
                // Specify the assembly containing your modules
                var modulesAssembly = assembly;
                var modules = modulesAssembly.GetTypes()
                    .Where(t => typeof(ICarterModule).IsAssignableFrom(t) && !t.IsAbstract)
                    .ToArray();
                c.WithModules(modules);
            });
            return services;
        }


        #endregion

        #region MediaTR Config
        //public static IServiceCollection AddAuthMediaTR(this IServiceCollection services, Assembly assembly)
        //{
        //    // Add MediatR
        //    services.AddMediatR(config =>
        //    {
        //        config.RegisterServicesFromAssembly(assembly);
        //        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        //        config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        //    });
        //    return services;
        //}
        #endregion

        #region Services
        public static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddTransient<IPasswordHasherService, PasswordHasherService>();
            //services.AddTransient<InitialData>();
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
