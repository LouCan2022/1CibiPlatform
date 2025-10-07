namespace Auth.Data.Extensions;

public static class AuthDatabaseExtensions
{
	public static async Task AuthIntializeDatabaseAsync(this WebApplication app)
	{
		using var scope = app.Services.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<AuthApplicationDbContext>();
		var initData = scope.ServiceProvider.GetRequiredService<AuthInitialData>();
		context.Database.MigrateAsync().GetAwaiter().GetResult();
		await SeedAsync(context, initData);
	}
	private static async Task SeedAsync(
		AuthApplicationDbContext context,
		AuthInitialData initData)
	{
		await SeedUser(context, initData);
	}
	private static async Task SeedUser(
		AuthApplicationDbContext context,
		AuthInitialData initData)
	{
		if (!await context.AuthUsers.AnyAsync())
		{
			await context.AuthUsers.AddRangeAsync(initData.GetUsers());
		}
		if (!await context.AuthApplications.AnyAsync())
		{
			await context.AuthApplications.AddRangeAsync(initData.GetApplications());
		}
		if (!await context.AuthRoles.AnyAsync())
		{
			await context.AuthRoles.AddRangeAsync(initData.GetRoles());
		}
		if (!await context.AuthUserAppRoles.AnyAsync())
		{
			await context.AuthUserAppRoles.AddRangeAsync(initData.GetUserAppRoles());
		}
		if (!await context.AuthSubmenu.AnyAsync())
		{
			await context.AuthSubmenu.AddRangeAsync(initData.GetSubMenus());
		}
		await context.SaveChangesAsync();
	}
}
