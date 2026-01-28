namespace AIAgent.Data.Extensions;

public static class AIAgentDatabaseExtensions
{
	public static async Task AIAgentIntializeDatabaseAsync(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();

		var context = scope.ServiceProvider.GetRequiredService<AIAgentApplicationDBContext>();

		context.Database.MigrateAsync().GetAwaiter().GetResult();
	}

}
