namespace APIs.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task IntializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthApplicationDbContext>();
        var initData = scope.ServiceProvider.GetRequiredService<InitialData>();

        context.Database.MigrateAsync().GetAwaiter().GetResult();
        await SeedAsync(context, initData);
    }

    private static async Task SeedAsync(
        AuthApplicationDbContext context,
        InitialData initData)
    {
        await SeedUser(context, initData);
    }

    private static async Task SeedUser(
        AuthApplicationDbContext context,
        InitialData initData)
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

        await context.SaveChangesAsync();
    }
}
