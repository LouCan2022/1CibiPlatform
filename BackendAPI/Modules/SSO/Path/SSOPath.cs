namespace SSO.Path;

// This class provides route and cluster declarations for the SSO module.
// Comments explain how to use Metadata and Methods:
// - Use `Methods` to restrict allowed HTTP methods on the gateway (e.g. new[] { "GET" }).
// - Use `Metadata` to attach route-specific values such as rate-limit policy names or flags.
// Example: Metadata = new Dictionary<string,string> { { "RateLimitPolicy", "LoginPolicy" } }
// The gateway will read these DTOs at startup and convert them to YARP route/cluster configs.
public class SSOPath : IReverseProxyModule
{

	public IEnumerable<RouteDefinitionDTO> GetRoutes()
	{
		return new[]
		{
			new RouteDefinitionDTO(
				RouteId: "SSOLoginEntryPoint",
				MatchPath: "sso/login",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new[] { GatewayConstants.HttpMethod.Get },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "sso/login" }
				}
			),

			new RouteDefinitionDTO(
				RouteId: "SSOLoginCallBackEntryPoint",
				MatchPath: "sso/login/callback",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new[] { GatewayConstants.HttpMethod.Get },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "sso/login/callback" }
				}
			),

			new RouteDefinitionDTO(
				RouteId: "SSOIsAuthenticatedEntryPoint",
				MatchPath: "sso/is-user-authenticated",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new[] { GatewayConstants.HttpMethod.Get },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "sso/is-user-authenticated" }
				}
			),

			new RouteDefinitionDTO(
				RouteId: "SSOLogoutBackEntryPoint",
				MatchPath: "sso/logout",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new[] { GatewayConstants.HttpMethod.Post },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "sso/logout" }
				}
			),

			new RouteDefinitionDTO(
				RouteId: "SSOSaml2EntryPoint",
				MatchPath: "Saml2",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new[] { GatewayConstants.HttpMethod.Post },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "Saml2" }
				}
			),

			new RouteDefinitionDTO(
				RouteId: "SSOSaml2ACSEntryPoint",
				MatchPath: "Saml2/Acs",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new[] { GatewayConstants.HttpMethod.Post },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "Saml2/Acs" }
				}
			)
		};
	}

	public IEnumerable<ClusterDefinitionDTO> GetClusters()
	{
		return Enumerable.Empty<ClusterDefinitionDTO>();
	}
}