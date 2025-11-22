namespace BuildingBlocks.SharedDTO;

// DTO used by modules to declare routes for the gateway.
// - MatchPath maps to YARP route Match.Path
// - Methods (optional) maps to YARP route Match.Methods (e.g. ["GET","POST"]).
public record RouteDefinitionDTO(
 string RouteId,
 string MatchPath,
 string ClusterId,
 IEnumerable<string>? Methods = null,
 IDictionary<string, string>? Transforms = null,
 IDictionary<string, string>? Metadata = null
);
