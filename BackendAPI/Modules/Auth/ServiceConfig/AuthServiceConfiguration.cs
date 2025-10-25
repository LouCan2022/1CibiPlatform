﻿using BuildingBlocks.SharedServices.Implementations;

namespace Auth.ServiceConfig
{
	public static class AuthServiceConfiguration
	{
		private const string assemblyName = "APIs";
		private const string connStringSegment = "OnePlatform_Connection";


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
			services.AddHttpContextAccessor();
			services.AddTransient<AuthInitialData>();
			services.AddTransient<IPasswordHasherService, PasswordHasherService>();
			services.AddScoped<IJWTService, JWTService>();
			services.AddScoped<IAuthRepository, AuthRepository>();
			services.AddScoped<IRefreshTokenService, RefreshTokenService>();
			services.AddScoped<ILoginService, LoginService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IOtpService, OtpService>();
			services.AddScoped<IHashService, HashService>();
			services.AddScoped<IRegisterService, RegisterService>();
			services.AddScoped<IForgotPassword, ForgotPassword>();
			services.AddScoped<ISecureToken, SecureToken>();


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
