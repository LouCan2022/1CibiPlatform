namespace CNX.Data.Extensions;

public static class CNXDatabaseExtensions
{
    public static async Task CNXIntializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CNXApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
}
