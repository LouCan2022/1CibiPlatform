﻿namespace APIs.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task IntializeDatabaseAsync(this WebApplication app)
    {
        AuthDatabaseExtensions.AuthIntializeDatabaseAsync(app).GetAwaiter().GetResult();
        CNXDatabaseExtensions.CNXIntializeDatabaseAsync(app).GetAwaiter().GetResult();
    }
}
