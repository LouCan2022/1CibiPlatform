namespace AIAgent.Features.AskAI;

public record AskAIRequest(
	string UserId,
	string Question,
	string ExplicitSkillName,
	UploadedFileDto? UploadedFile = null);

public record AskAIResponse(AIAnswerDTO AiAnswer);

public record AskAIEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		// Support both JSON body and multipart/form-data with file
		app.MapPost("aiagent/askai", async (
			HttpRequest httpRequest,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			string userId = string.Empty;
			string question = string.Empty;
			string explicitSkillName = string.Empty;
			UploadedFileDto? uploaded = null;

			var contentType = httpRequest.ContentType ?? string.Empty;

			if (httpRequest.HasFormContentType)
			{
				// Handle multipart/form-data
				var form = await httpRequest.ReadFormAsync(cancellationToken);
				userId = form["UserId"].FirstOrDefault() ?? string.Empty;
				question = form["Question"].FirstOrDefault() ?? string.Empty;
				explicitSkillName = form["ExplicitSkillName"].FirstOrDefault() ?? string.Empty;

				var file = form.Files.FirstOrDefault();
				if (file is not null && !string.IsNullOrWhiteSpace(userId))
				{
					using var ms = new MemoryStream();
					await file.CopyToAsync(ms, cancellationToken);
					uploaded = new UploadedFileDto(file.FileName, ms.ToArray());
				}
			}

			else if (contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
			{
				// Handle application/json — only deserialize when request is JSON
				try
				{
					var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
					var jsonRequest = await JsonSerializer.DeserializeAsync<AskAIRequest>(
						httpRequest.Body,
						jsonOptions,
						cancellationToken);

					if (jsonRequest is null)
					{
						return Results.BadRequest("Invalid JSON body");
					}

					userId = jsonRequest.UserId;
					question = jsonRequest.Question;
					uploaded = jsonRequest.UploadedFile;
					explicitSkillName = jsonRequest.ExplicitSkillName;
				}
				catch (JsonException)
				{
					return Results.BadRequest("Invalid JSON body");
				}
			}
			else
			{
				// Unsupported media type
				return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
			}

			if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(question))
			{
				return Results.BadRequest("UserId and Question are required");
			}

			var query = new AskAIQueryRequest(userId, question, explicitSkillName, uploaded);

			var aiAnswerResult = await sender.Send(query, cancellationToken);

			var result = new AskAIResponse(aiAnswerResult.aiAnswerDTO);

			return Results.Ok(result);
		}).WithName("AskAI")
			.WithTags("AI Agent")
			.Produces<AskAIResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Ask AI a Question")
			.WithDescription("Sends a question to the AI agent and retrieves the answer. Supports optional file upload in multipart/form-data with fields")
			.RequireAuthorization();
	}
}