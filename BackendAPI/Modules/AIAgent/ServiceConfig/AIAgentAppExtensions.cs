namespace AIAgent.ServiceConfig;

public static class AIAgentAppExtensions
{
	public static WebApplication UseAIAgentSkills(this WebApplication app)
	{
		var loggerFactory = app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
		var logger = loggerFactory?.CreateLogger("AIAgentSkills") ?? NullLoggerFactory.Instance.CreateLogger("AIAgentSkills");

		try
		{
			var registry = app.Services.GetService(typeof(SkillRegistry)) as SkillRegistry;
			if (registry is null)
			{
				logger.LogInformation("SkillRegistry not registered; skipping skill scan.");
				return app;
			}

			var contentRoot = app.Environment.ContentRootPath ?? Directory.GetCurrentDirectory();
			logger.LogInformation("Content root path: {ContentRoot}", contentRoot);

			// Enhanced candidate locations for both development and Docker deployment
			var candidates = new[]
			{
				// Docker deployment paths (when published to /app)
				System.IO.Path.Combine(contentRoot, "Skills"),
				System.IO.Path.Combine(contentRoot, "Modules", "AIAgent", "Skills"),
				
				// Development paths
				System.IO.Path.Combine(contentRoot, "BackendAPI", "Modules", "AIAgent", "Skills"),
				System.IO.Path.Combine(contentRoot, "Modules", "AIAgent", "Skills"),
				System.IO.Path.Combine(contentRoot, "BackendAPI", "Modules", "AIAgent"),
				System.IO.Path.Combine(contentRoot, "Modules", "AIAgent"),
			};

			string? skillsDir = null;

			// Log all candidates being checked
			logger.LogInformation("Checking {Count} candidate paths for Skills directory", candidates.Length);
			foreach (var c in candidates)
			{
				logger.LogDebug("  Checking: {Path} - Exists: {Exists}", c, Directory.Exists(c));

				if (Directory.Exists(c))
				{
					// prefer explicit Skills folder if present
					if (c.EndsWith("Skills", StringComparison.OrdinalIgnoreCase))
					{
						skillsDir = c;
						logger.LogInformation("Found Skills directory at: {Path}", skillsDir);
						break;
					}

					// otherwise if parent exists, try find Skills subfolder
					var trySkills = System.IO.Path.Combine(c, "Skills");
					if (Directory.Exists(trySkills))
					{
						skillsDir = trySkills;
						logger.LogInformation("Found Skills subdirectory at: {Path}", skillsDir);
						break;
					}
				}
			}

			// As a fallback, search entire content root for any .skill.yaml files
			if (skillsDir is null)
			{
				logger.LogInformation("Skills directory not found in candidates, searching entire content root for .skill.yaml files");

				var foundFiles = Directory.EnumerateFiles(contentRoot, "*.skill.yaml", SearchOption.AllDirectories).ToList();
				if (foundFiles.Any())
				{
					logger.LogInformation("Found {Count} .skill.yaml files under content root {Root}", foundFiles.Count, contentRoot);
					foreach (var f in foundFiles.Take(10)) // log up to 10 files
					{
						logger.LogInformation("  Skill manifest: {File}", f);
					}

					// choose the directory that contains the first file; prefer the containing Skills folder
					var first = foundFiles.First();
					var dir = System.IO.Path.GetDirectoryName(first) ?? contentRoot;

					// climb up until we find a folder named 'Skills' or stop at contentRoot
					var cur = dir;
					while (!string.Equals(System.IO.Path.GetFileName(cur), "Skills", StringComparison.OrdinalIgnoreCase) &&
						   cur != null &&
						   cur.Length >= contentRoot.Length)
					{
						var parent = System.IO.Path.GetDirectoryName(cur);
						if (string.IsNullOrEmpty(parent) || parent == cur) break;
						cur = parent;
						if (string.Equals(System.IO.Path.GetFileName(cur), "Skills", StringComparison.OrdinalIgnoreCase)) break;
					}

					if (cur is not null && Directory.Exists(cur) &&
						string.Equals(System.IO.Path.GetFileName(cur), "Skills", StringComparison.OrdinalIgnoreCase))
					{
						skillsDir = cur;
						logger.LogInformation("Resolved Skills directory from manifest location: {Path}", skillsDir);
					}
					else
					{
						// fallback to the directory of the first file
						skillsDir = dir;
						logger.LogInformation("Using directory of first manifest file: {Path}", skillsDir);
					}
				}
			}

			if (skillsDir is null || !Directory.Exists(skillsDir))
			{
				logger.LogWarning("Skills directory does not exist. Content root: {Root}, Skills dir: {SkillsDir}", contentRoot, skillsDir ?? "<none>");
				logger.LogInformation("Currently registered skills count: {Count}", registry.GetAll().Count());
				return app;
			}

			// List files for diagnostics
			var manifests = Directory.GetFiles(skillsDir, "*.skill.yaml", SearchOption.AllDirectories).ToList();
			logger.LogInformation("Using skills directory: {Dir}. Found {Count} manifests.", skillsDir, manifests.Count);

			if (!manifests.Any())
			{
				logger.LogWarning("No .skill.yaml manifests found in {Dir}", skillsDir);
				return app;
			}

			foreach (var f in manifests)
			{
				logger.LogInformation("  Skill manifest: {File}", f);
			}

			// Call ScanAndRegister on discovered skills directory
			registry.ScanAndRegister(skillsDir);

			var registeredCount = registry.GetAll().Count();
			logger.LogInformation("Registered {Count} skills from {Dir}", registeredCount, skillsDir);

			// Log each registered skill
			foreach (var skill in registry.GetAll())
			{
				logger.LogInformation("  Registered skill: {Name} - Implementation: {Type}",
					skill.Name,
					skill.ImplementationType?.Name ?? "<none>");
			}

			// Try kernel import via reflection
			var kernel = app.Services.GetService(typeof(Microsoft.SemanticKernel.Kernel));
			if (kernel is null)
			{
				logger.LogInformation("Semantic Kernel not available in DI; skipping import.");
				return app;
			}

			var kType = kernel.GetType();
			var importMethod = kType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.FirstOrDefault(m => m.Name.IndexOf("Import", StringComparison.OrdinalIgnoreCase) >= 0
						&& m.GetParameters().Length >= 1
						&& m.GetParameters()[0].ParameterType == typeof(string));

			if (importMethod is not null)
			{
				try
				{
					// Try import the whole directory first
					importMethod.Invoke(kernel, new object[] { skillsDir });
					logger.LogInformation("Invoked Kernel import method '{Method}' with {Dir}", importMethod.Name, skillsDir);
				}
				catch (TargetInvocationException tie)
				{
					logger.LogWarning(tie, "Kernel import method threw an exception when importing directory.");
				}
				catch (Exception ex)
				{
					logger.LogWarning(ex, "Failed to invoke kernel import method for directory.");
				}

				// Also attempt to import individual manifest files to maximize compatibility
				foreach (var desc in registry.GetAll())
				{
					try
					{
						if (string.IsNullOrWhiteSpace(desc.ManifestPath) || !File.Exists(desc.ManifestPath))
							continue;

						importMethod.Invoke(kernel, new object[] { desc.ManifestPath });
						logger.LogInformation("Imported skill manifest '{Manifest}' for skill '{Skill}'", desc.ManifestPath, desc.Name);
					}
					catch (Exception ex)
					{
						logger.LogWarning(ex, "Failed to import individual manifest for skill '{Skill}'", desc.Name);
					}
				}
			}
			else
			{
				logger.LogInformation("No suitable Kernel import method found; skipping automatic Kernel import.");
			}

			return app;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error during UseAIAgentSkills.");
			return app;
		}
	}
}
