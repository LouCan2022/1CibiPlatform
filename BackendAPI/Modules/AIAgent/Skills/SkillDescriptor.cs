namespace AIAgent.Skills;

public sealed class SkillDescriptor
{
	public string Name { get; init; } = string.Empty;
	public Type? ImplementationType { get; init; }
	public string? ManifestPath { get; init; }
	public string MethodName { get; init; } = "RunAsync";
}
