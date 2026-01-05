namespace AIAgent.Skills;

public interface ISkill
{
	// Implementations should accept a JsonElement payload matching the manifest schema and return a result object (or null).
	Task<object?> RunAsync(JsonElement payload, CancellationToken cancellationToken = default);
}
