using YamlDotNet.RepresentationModel;

namespace AIAgent.Skills;

public sealed class SkillRegistry
{
	private readonly IServiceProvider _services;
	private readonly ConcurrentDictionary<string, SkillDescriptor> _skills = new(StringComparer.OrdinalIgnoreCase);

	public SkillRegistry(IServiceProvider services)
	{
		_services = services;
	}

	public void Register(SkillDescriptor desc)
	{
		if (desc is null) throw new ArgumentNullException(nameof(desc));
		_skills[desc.Name] = desc;
	}

	public bool TryGet(string name, out SkillDescriptor? desc) => _skills.TryGetValue(name, out desc);

	public IEnumerable<SkillDescriptor> GetAll() => _skills.Values;

	// Scan a skills root directory for *.skill.yaml and register descriptors using improved mapping
	public void ScanAndRegister(string skillsRootDirectory)
	{
		if (string.IsNullOrWhiteSpace(skillsRootDirectory) || !System.IO.Directory.Exists(skillsRootDirectory))
			return;

		var yamlFiles = System.IO.Directory.GetFiles(skillsRootDirectory, "*.skill.yaml", SearchOption.AllDirectories);
		var assemblies = AppDomain.CurrentDomain.GetAssemblies();

		foreach (var yf in yamlFiles)
		{
			try
			{
				var folderName = new DirectoryInfo(System.IO.Path.GetDirectoryName(yf) ?? string.Empty).Name;
				var skillName = folderName; // use folder name as skill name

				// Read YAML and check for explicit implementation override
				string? implTypeName = null;
				try
				{
					var yamlText = System.IO.File.ReadAllText(yf);
					var input = new StringReader(yamlText);
					var yaml = new YamlStream();
					yaml.Load(input);
					var root = (YamlMappingNode)yaml.Documents[0].RootNode;
					if (root.Children.TryGetValue(new YamlScalarNode("implementation"), out var implNode))
					{
						implTypeName = implNode.ToString();
					}
				}
				catch
				{
					// ignore YAML parse errors; continue with convention
				}

				Type? implType = null;
				if (!string.IsNullOrWhiteSpace(implTypeName))
				{
					implType = assemblies.Select(a => a.GetType(implTypeName!, false, true)).FirstOrDefault(t => t is not null);
				}

				if (implType is null)
				{
					// fallback: find any type in assemblies that implements ISkill and lives in namespace AIAgent.Skills.{folderName}
					var ns = $"AIAgent.Skills.{folderName}";
					implType = assemblies.SelectMany(a => a.DefinedTypes)
						.Where(t => string.Equals(t.Namespace, ns, StringComparison.OrdinalIgnoreCase))
						.Where(t => t.ImplementedInterfaces.Any(i => i == typeof(ISkill)))
						.Select(t => t.AsType())
						.FirstOrDefault();

					if (implType is null)
					{
						// try any type that implements ISkill and has folderName in namespace
						implType = assemblies.SelectMany(a => a.DefinedTypes)
							.Where(t => t.ImplementedInterfaces.Any(i => i == typeof(ISkill)))
							.Select(t => t.AsType())
							.FirstOrDefault(t => t.Namespace?.IndexOf(folderName, StringComparison.OrdinalIgnoreCase) >= 0);
					}
				}

				var desc = new SkillDescriptor
				{
					Name = skillName,
					ManifestPath = yf,
					ImplementationType = implType
				};

				Register(desc);
			}
			catch
			{
				// ignore per-file errors
			}
		}
	}

	public async Task<object?> InvokeAsync(string name, JsonElement payload, CancellationToken cancellationToken = default)
	{
		if (!_skills.TryGetValue(name, out var desc) || desc?.ImplementationType is null)
			throw new InvalidOperationException($"Skill '{name}' not registered or has no implementation.");

		using var scope = _services.CreateScope();
		var implInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, desc.ImplementationType!);
		if (implInstance is null)
			throw new InvalidOperationException($"Failed to create instance of '{desc.ImplementationType}'.");

		// If the implementation implements ISkill, call RunAsync(JsonElement)
		if (implInstance is ISkill skillImpl)
		{
			return await skillImpl.RunAsync(payload, cancellationToken).ConfigureAwait(false);
		}

		// Otherwise try to find a typed RunAsync method and deserialize
		var method = desc.ImplementationType!.GetMethod(desc.MethodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
		if (method is null)
			throw new InvalidOperationException($"Method '{desc.MethodName}' not found on type '{desc.ImplementationType}'.");

		var parameters = method.GetParameters();
		object?[] invokeArgs;

		if (parameters.Length == 0)
		{
			invokeArgs = Array.Empty<object?>();
		}
		else
		{
			var firstParamType = parameters[0].ParameterType;
			var inputObj = JsonSerializer.Deserialize(payload.GetRawText(), firstParamType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (parameters.Length == 1)
			{
				invokeArgs = new object?[] { inputObj };
			}
			else if (parameters.Length == 2 && parameters[1].ParameterType == typeof(CancellationToken))
			{
				invokeArgs = new object?[] { inputObj, cancellationToken };
			}
			else
			{
				throw new InvalidOperationException("Skill RunAsync method has unsupported signature. Use RunAsync(InputType) or RunAsync(InputType, CancellationToken).");
			}
		}

		var taskObj = method.Invoke(implInstance, invokeArgs);
		if (taskObj is not Task task)
			return taskObj;

		await task.ConfigureAwait(false);

		var taskType = task.GetType();
		if (taskType.IsGenericType)
		{
			var resultProp = taskType.GetProperty("Result");
			return resultProp?.GetValue(task);
		}

		return null;
	}
}
