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

			// Candidate locations relative to content root
			var contentRoot = app.Environment.ContentRootPath ?? Directory.GetCurrentDirectory();

			var candidates = new[]
			{
				System.IO.Path.Combine(contentRoot, "BackendAPI", "Modules", "AIAgent", "Skills"),
				System.IO.Path.Combine(contentRoot, "Modules", "AIAgent", "Skills"),
				System.IO.Path.Combine(contentRoot, "BackendAPI", "Modules", "AIAgent"),
				System.IO.Path.Combine(contentRoot, "Modules", "AIAgent"),
			};

			string? skillsDir = null;

			foreach (var c in candidates)
			{
				if (Directory.Exists(c))
				{
					// prefer explicit Skills folder if present
					if (c.EndsWith(System.IO.Path.Combine("AIAgent", "Skills")))
					{
						skillsDir = c;
						break;
					}

					// otherwise if parent exists, try find Skills subfolder
					var trySkills = System.IO.Path.Combine(c, "Skills");
					if (Directory.Exists(trySkills))
					{
						skillsDir = trySkills;
						break;
					}
				}
			}

			// As a fallback, search entire content root for any .skill.yaml files
			if (skillsDir is null)
			{
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
					while (!string.Equals(System.IO.Path.GetFileName(cur), "Skills", StringComparison.OrdinalIgnoreCase) && cur != null && cur.Length >= contentRoot.Length)
					{
						var parent = System.IO.Path.GetDirectoryName(cur);
						if (string.IsNullOrEmpty(parent) || parent == cur) break;
						cur = parent;
						if (string.Equals(System.IO.Path.GetFileName(cur), "Skills", StringComparison.OrdinalIgnoreCase)) break;
					}

					if (cur is not null && Directory.Exists(cur) && System.IO.Path.GetFileName(cur).Equals("Skills", StringComparison.OrdinalIgnoreCase))
					{
						skillsDir = cur;
					}
					else
					{
						// fallback to the directory of the first file
						skillsDir = dir;
					}
				}
			}

			if (skillsDir is null || !Directory.Exists(skillsDir))
			{
				logger.LogWarning("Skills directory does not exist (checked candidates and content root): {Root}", contentRoot);
				logger.LogInformation("Registered {Count} skill manifests from {Dir}", registry.GetAll().Count(), skillsDir ?? "<none>");
				return app;
			}

			// List files for diagnostics
			var manifests = Directory.GetFiles(skillsDir, "*.skill.yaml", SearchOption.AllDirectories).ToList();
			logger.LogInformation("Using skills directory: {Dir}. Found {Count} manifests.", skillsDir, manifests.Count);
			foreach (var f in manifests)
			{
				logger.LogInformation("  Skill manifest: {File}", f);
			}

			// Call ScanAndRegister on discovered skills directory
			registry.ScanAndRegister(skillsDir);

			logger.LogInformation("Registered {Count} skill manifests from {Dir}", registry.GetAll().Count(), skillsDir);

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
					catch (TargetInvocationException tie)
					{
						logger.LogWarning(tie, "Kernel import method threw when importing manifest {Manifest}", desc.ManifestPath);
					}
					catch (Exception ex)
					{
						logger.LogWarning(ex, "Failed to import manifest {Manifest}", desc.ManifestPath);
					}
				}
			}
			else
			{
				logger.LogInformation("No kernel import method found. You can register adapters manually using SkillRegistry.");
			}
		}
		catch (Exception ex)
		{
			var logger2 = app.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
			logger2?.CreateLogger("AIAgentSkills").LogError(ex, "Error during skill scan/import");
		}

		return app;
	}
}
