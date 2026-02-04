namespace AIAgent.ServiceConfig;

public static class AIAgentAppExtensions
{
	/// <summary>
	/// Finds the common parent directory that contains all the given directories.
	/// </summary>
	private static string FindCommonParentDirectory(List<string> directories, string fallback)
	{
		if (!directories.Any()) return fallback;
		if (directories.Count == 1) return directories[0];

		// Normalize paths and split into parts
		var pathParts = directories
			.Select(d => d.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar))
			.Select(d => d.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
			.ToList();

		// Find shortest path length
		var minLength = pathParts.Min(p => p.Length);

		// Find common prefix
		var commonParts = new List<string>();
		for (int i = 0; i < minLength; i++)
		{
			var part = pathParts[0][i];
			if (pathParts.All(p => string.Equals(p[i], part, StringComparison.OrdinalIgnoreCase)))
			{
				commonParts.Add(part);
			}
			else
			{
				break;
			}
		}

		if (!commonParts.Any()) return fallback;

		// Reconstruct path
		var result = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), commonParts);

		// Handle absolute paths (add leading separator if original paths were absolute)
		if (directories[0].StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ||
			directories[0].StartsWith(System.IO.Path.AltDirectorySeparatorChar.ToString()))
		{
			result = System.IO.Path.DirectorySeparatorChar + result;
		}

		return Directory.Exists(result) ? result : fallback;
	}

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
			};

			string? skillsDir = null;

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

					// Strategy: Find the best common directory that contains all skill files
					// 1. First, check if any file is under a folder named "Skills" - use that
					// 2. Otherwise, find the common parent directory of all files
					string? commonDir = null;

					// Look for a "Skills" directory among the file paths
					foreach (var file in foundFiles)
					{
						var dir = System.IO.Path.GetDirectoryName(file);
						if (dir is null) continue;

						// Check if "Skills" is in the path
						var parts = dir.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
						var skillsIndex = Array.FindIndex(parts, p => string.Equals(p, "Skills", StringComparison.OrdinalIgnoreCase));

						if (skillsIndex >= 0)
						{
							// Reconstruct path up to and including "Skills"
							var skillsPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), parts.Take(skillsIndex + 1));
							if (Directory.Exists(skillsPath))
							{
								commonDir = skillsPath;
								logger.LogInformation("Found 'Skills' directory in path: {Path}", commonDir);
								break;
							}
						}
					}

					// If no "Skills" directory found, find common parent of all files
					if (commonDir is null && foundFiles.Any())
					{
						var allDirs = foundFiles.Select(f => System.IO.Path.GetDirectoryName(f))
							.Where(d => d is not null)
							.Select(d => d!)
							.Distinct()
							.ToList();

						if (allDirs.Count == 1)
						{
							// All files in same directory
							commonDir = allDirs[0];
							logger.LogInformation("All skill files in same directory: {Path}", commonDir);
						}
						else
						{
							// Find common parent directory
							commonDir = FindCommonParentDirectory(allDirs, contentRoot);
							logger.LogInformation("Found common parent directory for {Count} skill locations: {Path}", allDirs.Count, commonDir);
						}
					}

					skillsDir = commonDir ?? contentRoot;
					logger.LogInformation("Using skills directory: {Path}", skillsDir);
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

