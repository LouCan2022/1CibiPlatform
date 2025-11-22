using BuildingBlocks.SharedDTO;

namespace BuildingBlocks.SharedInterfaces;

public interface IReverseProxyModule
{
	// Return route definitions owned by this module
    IEnumerable<RouteDefinitionDTO> GetRoutes();


	// Return cluster(s) this module depends on
	IEnumerable<ClusterDefinitionDTO> GetClusters();
}
