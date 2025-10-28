﻿namespace APIs.ServiceConfig
{
	public static class ServiceConfiguration
	{

		private static readonly Assembly _authAssembly = typeof(AuthMarker).Assembly;
		private static readonly Assembly _cnxAssembly = typeof(CNXMarker).Assembly;
		private static readonly Assembly _philsysAssembly = typeof(PhilSysMarker).Assembly;
		private static readonly Assembly _ssoAssembly = typeof(SSOMarker).Assembly;


		#region Environment Config

		public static void ConfigureEnvironment(
			this IServiceCollection services,
			WebApplicationBuilder builder)
		{
			if (builder.Environment.IsDevelopment())
			{
				builder.Services.ConfigureCorsDev();
			}

			if (builder.Environment.IsProduction())
			{
				builder.Services.ConfigureCorsProd();
			}
		}


		#endregion

		#region CORS
		public static void ConfigureCorsProd(this IServiceCollection services) =>
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

		public static void ConfigureCorsDev(this IServiceCollection services) =>
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
								 policy =>
								 {
									 policy.WithOrigins(
										 "http://localhost:5123",
										 "http://localhost:5134")
										   .AllowAnyHeader()
										   .AllowAnyMethod()
										   .AllowCredentials();
								 });
			});
		#endregion

		#region JWT Config
		public static IServiceCollection AddJwtAuthentication(
			this IServiceCollection services,
			IConfiguration configuration,
			IHostEnvironment environment)
		{
			// JWT Authentication
			var jwtSettings = configuration.GetSection("Jwt");
			var key = jwtSettings["Key"];
			var issuer = jwtSettings["Issuer"];
			var audience = jwtSettings["Audience"];
			var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);
			var _httpCookieOnlyKey = configuration.GetValue<string>("HttpCookieOnlyKey");
			var _signinScheme = configuration.GetValue<string>("SSOMetadata:SigninScheme");
			// SAML2 Configuration
			var spBaseUrl = configuration["Saml2:SpBaseUrl"];
			var idpMetadataUrl = configuration["Saml2:IdpMetadataUrl"];
			var idpEntityId = configuration["Saml2:IdpEntityId"];

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultSignInScheme = _signinScheme;
				x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
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
				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						if (context.Request.Cookies.TryGetValue(_httpCookieOnlyKey!, out var token))
						{
							context.Token = token;
						}
						return Task.CompletedTask;
					}
				};
			})
			.AddCookie(_signinScheme!, options =>
			{
				options.Cookie.Name = _signinScheme;
				options.Cookie.HttpOnly = true;
				options.Cookie.Path = "/";
				options.ExpireTimeSpan = TimeSpan.FromHours(8);
				options.SlidingExpiration = true;

				// Environment-specific settings
				if (environment.IsDevelopment())
				{
					// Development/Ngrok settings
					options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
					options.Cookie.SameSite = SameSiteMode.None;
				}
				else
				{
					options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
					options.Cookie.SameSite = SameSiteMode.Lax;
					// Optionally set domain for subdomain sharing
					// options.Cookie.Domain = ".yourdomain.com";
				}
			})
			.AddSaml2(options =>
			{
				options.SPOptions.EntityId = new EntityId(spBaseUrl);
				options.SPOptions.ReturnUrl = new Uri(spBaseUrl + "/sso/login/callback");
				var identityProvider = new IdentityProvider(
					new EntityId(idpEntityId),
					options.SPOptions)
				{
					MetadataLocation = idpMetadataUrl,
					LoadMetadata = true,
					AllowUnsolicitedAuthnResponse = true
				};
				options.IdentityProviders.Add(identityProvider);
			});

			services.AddOptions<Saml2Options>().ValidateOnStart();
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
			services.AddCNXInfrastructure(configuration);
			services.AddPhilSysInfrastructure(configuration);
			return services;
		}
		#endregion

		#region Carter Config
		public static IServiceCollection AddModuleCarter(this IServiceCollection services)
		{
			services.AddCarter(new DependencyContextAssemblyCatalog([
				 _authAssembly,
				 _cnxAssembly,
				 _philsysAssembly,
				 _ssoAssembly
			 ]));


			return services;
		}
		#endregion

		#region MediaTR Config

		public static IServiceCollection AddModuleMediaTR(
			this IServiceCollection services)
		{
			// Add MediaTR
			services.AddAuthMediaTR(_authAssembly);
			services.AddCNXMediaTR(_cnxAssembly);
			services.AddPhilSysMediaTR(_philsysAssembly);
			services.AddSSOMediaTR(_ssoAssembly);
			return services;
		}

		#endregion

		#region Services Config
		public static IServiceCollection AddModuleServices(this IServiceCollection services)
		{
			// Add Services
			services.AddAuthServices();
			services.AddCNXServices();
			services.AddPhilSysServices();
			services.AddSSOServices();
			return services;
		}
		#endregion
	}
}
