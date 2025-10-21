namespace APIs.ServiceConfig;

public static class AppConfiguration
{
	#region CORS
	public static async Task<WebApplication> UseEnvironmentAsync(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			await DatabaseExtensions.IntializeDatabaseAsync(app);
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		if (app.Environment.IsProduction())
		{
			await DatabaseExtensions.IntializeDatabaseAsync(app);
		}

		return app;
	}


	#endregion

	#region Custom Middlewares
	public static WebApplication UseCustomMiddlewares(this WebApplication app)
	{
		app.MapControllers();

		app.UseExceptionHandler(options => { })
		   .UseHttpsRedirection()
		   .UseAuthentication()
		   .UseAuthorization();

		app.UseCors("CorsPolicy");

		app.MapCarter();

		return app;
	}
	#endregion


}
