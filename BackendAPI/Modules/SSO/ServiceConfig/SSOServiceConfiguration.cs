namespace SSO.ServiceConfig;

public static class SSOServiceConfiguration
{

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
}
