namespace AIAgent.Features.DownloadFile;

public class DownloadFileEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapGet("/api/files/{fileName}", async (string fileName, IConfiguration configuration) =>
		{
			var storageDirectory = configuration.GetValue<string>("FileStorage:Directory") ?? System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
			var filePath = System.IO.Path.Combine(storageDirectory, fileName);

			if (!File.Exists(filePath))
			{
				return Results.NotFound(new { Message = "File not found" });
			}

			var fileBytes = await File.ReadAllBytesAsync(filePath);
			return Results.File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
		})
		.WithName("DownloadFile")
		.WithTags("AIAgent")
		.Produces(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound)
		.RequireAuthorization();
	}
}
