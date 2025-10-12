namespace SSO.ServiceConfig;

public static class SSOServiceConfiguration
{
	#region MediaTR Config
	public static IServiceCollection AddSSOMediaTR(this IServiceCollection services, Assembly assembly)
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
	public static IServiceCollection AddSSOServices(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();
		return services;
	}

	#endregion

	#region SSO Config
	public static IServiceCollection AddSSOSamlConfiguration(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Configure SAML2
		var spBaseUrl = configuration["Saml2:SpBaseUrl"];
		var idpMetadataUrl = configuration["Saml2:IdpMetadataUrl"];
		var idpEntityId = configuration["Saml2:IdpEntityId"];

		// Add SAML2 authentication (JWT will be sent to frontend)
		services.AddAuthentication()
				.AddCookie("AppExternalScheme") // Temporary cookie for SAML flow
				.AddSaml2(sharedOptions =>
				{
					// Service Provider configuration
					sharedOptions.SPOptions.EntityId = new EntityId(spBaseUrl + "/Saml2");

					// Register Identity Provider
					var identityProvider = new IdentityProvider(
						new EntityId(idpEntityId),
						sharedOptions.SPOptions)
					{
						MetadataLocation = idpMetadataUrl,
						LoadMetadata = true,
						AllowUnsolicitedAuthnResponse = true
					};

					sharedOptions.IdentityProviders.Add(identityProvider);
				});



		return services;
	}
	#endregion
}
