using BuildingBlocks.SharedDTO;

namespace ApiGateways.YarpApiGateway.Services;

public class RouteCatalog
{
 public IReadOnlyList<RouteDefinitionDTO> Routes { get; init; } = Array.Empty<RouteDefinitionDTO>();
 public IReadOnlyList<ClusterDefinitionDTO> Clusters { get; init; } = Array.Empty<ClusterDefinitionDTO>();
}
