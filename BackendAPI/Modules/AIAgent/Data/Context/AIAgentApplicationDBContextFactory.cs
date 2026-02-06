using IOPath = System.IO.Path;

namespace AIAgent.Data.Context;

public class AIAgentApplicationDBContextFactory : IDesignTimeDbContextFactory<AIAgentApplicationDBContext>
{
	public AIAgentApplicationDBContext CreateDbContext(string[] args)
	{
		// Navigate to the solution root and find the appsettings file
		var currentDirectory = Directory.GetCurrentDirectory();
		var basePath = FindAppsettingsPath(currentDirectory);

		var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

		var configuration = new ConfigurationBuilder()
			.SetBasePath(basePath)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{environmentName}.json", optional: true)
			.Build();

		var connectionString = configuration.GetConnectionString("OnePlatform_Connection");

		var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
		dataSourceBuilder.UseVector();
		var dataSource = dataSourceBuilder.Build();

		var optionsBuilder = new DbContextOptionsBuilder<AIAgentApplicationDBContext>();
		optionsBuilder.UseNpgsql(
			dataSource,
			npgsqlOptions =>
			{
				npgsqlOptions.MigrationsAssembly("APIs");
				npgsqlOptions.UseVector();
			}
		);

		return new AIAgentApplicationDBContext(optionsBuilder.Options);
	}

	private static string FindAppsettingsPath(string startPath)
	{
		// Try direct path from solution root
		var directPath = IOPath.Combine(startPath, "BackendAPI", "API", "APIs");
		if (File.Exists(IOPath.Combine(directPath, "appsettings.Development.json")))
			return directPath;

		// Navigate up to find the solution root, then go to APIs folder
		var directory = new DirectoryInfo(startPath);
		while (directory != null)
		{
			var apisPath = IOPath.Combine(directory.FullName, "BackendAPI", "API", "APIs");
			if (File.Exists(IOPath.Combine(apisPath, "appsettings.Development.json")))
				return apisPath;

			directory = directory.Parent;
		}

		throw new FileNotFoundException("Could not find appsettings.Development.json. Make sure you run migrations from the solution root.");
	}
}
