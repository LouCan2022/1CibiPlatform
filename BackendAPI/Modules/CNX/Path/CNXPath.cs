namespace BackendAPI.Modules.CNX.Path;

public class CNXPath : IReverseProxyModule
{
	public IEnumerable<RouteDefinitionDTO> GetRoutes()
	{
		return new[]
		{
			new RouteDefinitionDTO(
			RouteId: "SearchCandidateEntryPoint",
			MatchPath: "cnx/gettalkpushcandidate",
			ClusterId: GatewayConstants.OnePlatformApi,
			Methods: new [] { GatewayConstants.HttpMethod.Get },
			Transforms: new Dictionary<string, string>
			{
				{ "PathSet", "cnx/gettalkpushcandidate" }
			}
			),
		};
	}
	public IEnumerable<ClusterDefinitionDTO> GetClusters()
	{
		return Enumerable.Empty<ClusterDefinitionDTO>();
	}
}
