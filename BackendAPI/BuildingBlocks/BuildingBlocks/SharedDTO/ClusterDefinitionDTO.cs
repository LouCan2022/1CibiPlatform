namespace BuildingBlocks.SharedDTO;

public record DestinationDefinitionDTO(string Id, string Address);


public record ClusterDefinitionDTO(
    string ClusterId,
    IEnumerable<DestinationDefinitionDTO> Destinations
);