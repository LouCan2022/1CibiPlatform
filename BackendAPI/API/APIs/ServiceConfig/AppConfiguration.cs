namespace APIs.ServiceConfig;

public static class AppConfiguration
{
	#region CORS
	public static async Task<WebApplication> UseEnvironmentAsync(this WebApplication app)
	{
		app.UseForwardedHeaders();

		if (app.Environment.IsEnvironment("Testing"))
		{
			return app;
		}

		if (app.Environment.IsDevelopment())
		{
			await DatabaseExtensions.IntializeDatabaseAsync(app);
			app.UseSwagger();
			app.UseSwaggerUI();

		}

		if (app.Environment.IsProduction())
		{
			await DatabaseExtensions.IntializeDatabaseAsync(app);
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		return app;
	}


	#endregion

	#region Custom Middlewares
	public static WebApplication UseCustomMiddlewares(this WebApplication app)
	{
		app.UseExceptionHandler(options => { });
		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseCors("CorsPolicy");
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();
		app.MapCarter();

		return app;
	}
	#endregion

}
