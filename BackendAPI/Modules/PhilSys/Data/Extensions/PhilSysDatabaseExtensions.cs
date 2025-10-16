namespace PhilSys.Data.Extensions;
public static class PhilSysDatabaseExtensions
{
	public static async Task PhilSysIntializeDatabaseAsync(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<PhilSysDBContext>();
		await context.Database.MigrateAsync();
	}
}
