namespace AIAgent.Skills;

public sealed class SkillRegistry
{
	private readonly IServiceProvider _services;
	private readonly ILogger<SkillRegistry> _logger;
	private readonly ConcurrentDictionary<string, SkillDescriptor> _skills = new(StringComparer.OrdinalIgnoreCase);

	public SkillRegistry(IServiceProvider services, ILogger<SkillRegistry> logger)
	{
		_services = services;
		_logger = logger;
	}

	public void Register(SkillDescriptor desc)
	{
		if (desc is null) throw new ArgumentNullException(nameof(desc));
		_skills[desc.Name] = desc;
		_logger.LogInformation("Registered skill: {SkillName} with implementation: {ImplementationType}",
			desc.Name,
			desc.ImplementationType?.FullName ?? "None");
	}

	public bool TryGet(string name, out SkillDescriptor? desc) => _skills.TryGetValue(name, out desc);

	public IEnumerable<SkillDescriptor> GetAll() => _skills.Values;

	/// <summary>
	/// Diagnostic method to log all currently registered skills
	/// </summary>
	public void LogRegisteredSkills()
	{
		_logger.LogInformation("?? Currently registered skills: {Count}", _skills.Count);
		if (_skills.Count == 0)
		{
			_logger.LogWarning("?? No skills are currently registered!");
		}
		else
		{
			foreach (var kvp in _skills)
			{
				_logger.LogInformation("  • [{Key}] ? {Name} | Type: {Type} | Manifest: {Manifest}",
					kvp.Key,
					kvp.Value.Name,
					kvp.Value.ImplementationType?.FullName ?? "? No Implementation",
					kvp.Value.ManifestPath ?? "? No Manifest");
			}
		}
	}

	// Scan a skills root directory for *.skill.yaml and register descriptors using improved mapping
	public void ScanAndRegister(string skillsRootDirectory)
	{
		if (string.IsNullOrWhiteSpace(skillsRootDirectory) || !System.IO.Directory.Exists(skillsRootDirectory))
		{
			_logger.LogWarning("Skills root directory is invalid or does not exist: {Directory}", skillsRootDirectory);
			return;
		}

		_logger.LogInformation("?? Scanning for skills in: {Directory}", skillsRootDirectory);
		var yamlFiles = System.IO.Directory.GetFiles(skillsRootDirectory, "*.skill.yaml", SearchOption.AllDirectories);
		_logger.LogInformation("?? Found {Count} skill YAML files:", yamlFiles.Length);
		foreach (var yf in yamlFiles)
		{
			_logger.LogInformation("  - {YamlFile}", yf);
		}

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();

		foreach (var yf in yamlFiles)
		{
			try
			{
				var folderName = new DirectoryInfo(System.IO.Path.GetDirectoryName(yf) ?? string.Empty).Name;
				var skillName = folderName; // use folder name as skill name

				_logger.LogDebug("Processing skill YAML: {YamlFile}, Folder: {FolderName}", yf, folderName);

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
						_logger.LogDebug("Found explicit implementation in YAML: {ImplType}", implTypeName);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to parse YAML for skill {SkillName}, continuing with convention-based lookup", skillName);
				}

				Type? implType = null;
				if (!string.IsNullOrWhiteSpace(implTypeName))
				{
					implType = assemblies.Select(a => a.GetType(implTypeName!, false, true)).FirstOrDefault(t => t is not null);
					if (implType is not null)
					{
						_logger.LogDebug("Found implementation type from YAML: {TypeName}", implType.FullName);
					}
					else
					{
						_logger.LogWarning("Explicit implementation type {TypeName} not found in loaded assemblies", implTypeName);
					}
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
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to register skill from YAML file: {YamlFile}", yf);
			}
		}

		_logger.LogInformation("Skill registration complete. Total skills registered: {Count}", _skills.Count);
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
