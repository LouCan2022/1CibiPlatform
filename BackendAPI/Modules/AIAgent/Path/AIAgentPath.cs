namespace AIAgent.Path;

public class AIAgentPath : IReverseProxyModule
{

	public IEnumerable<RouteDefinitionDTO> GetRoutes()
	{
		return new[]
		{
			new RouteDefinitionDTO(
				RouteId: "AskAIEntryPoint",
				MatchPath: "aiagent/ask",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new [] { GatewayConstants.HttpMethod.Post },
				Transforms: new Dictionary<string, string>
				{
					{ "PathSet", "aiagent/askai" }
				}
			),
		};
	}

	public IEnumerable<ClusterDefinitionDTO> GetClusters()
	{
		return Enumerable.Empty<ClusterDefinitionDTO>();
	}

}
