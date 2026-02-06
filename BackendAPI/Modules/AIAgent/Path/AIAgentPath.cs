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

			new RouteDefinitionDTO(
				RouteId: "GetAIAgentResponseEntryPoint",
				MatchPath: "hubs/aiagent/{**catch-all}",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new [] { GatewayConstants.HttpMethod.Post , GatewayConstants.HttpMethod.Get }
			),

			new RouteDefinitionDTO(
				RouteId: "DownloadFile",
				MatchPath: "api/files/{**catch-all}",
				ClusterId: GatewayConstants.OnePlatformApi,
				Methods: new [] { GatewayConstants.HttpMethod.Get }
			),

		};
	}

	public IEnumerable<ClusterDefinitionDTO> GetClusters()
	{
		return Enumerable.Empty<ClusterDefinitionDTO>();
	}

}
