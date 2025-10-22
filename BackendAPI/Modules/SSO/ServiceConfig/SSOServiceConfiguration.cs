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
		var spBaseUrl = configuration["Saml2:SpBaseUrl"];
		var idpMetadataUrl = configuration["Saml2:IdpMetadataUrl"];
		var idpEntityId = configuration["Saml2:IdpEntityId"];

		// Don't call AddAuthentication() - just add SAML2 to existing auth
		var authBuilder = new AuthenticationBuilder(services);

		authBuilder.AddCookie("AppExternalScheme")
				   .AddSaml2(options =>
				   {
					   options.SPOptions.EntityId = new EntityId(spBaseUrl);

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

		return services;
	}
	#endregion
}
