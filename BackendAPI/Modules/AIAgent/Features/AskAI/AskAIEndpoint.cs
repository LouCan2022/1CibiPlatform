namespace AIAgent.Features.AskAI;

public record AskAIRequest(string UserId, string Question);

public record AskAIResponse(AIAnswerDTO AiAnswer);

public record AskAIEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("aiagent/askai", async (
			AskAIRequest request,
			ISender sender,
			CancellationToken cancellationToken) =>
		{
			var query = new AskAIQueryRequest(
				request.UserId,
				request.Question);

			var aiAnswerResult = await sender.Send(query, cancellationToken);

			var result = new AskAIResponse(aiAnswerResult.aiAnswerDTO);

			return Results.Ok(result);
		})
		.WithName("AskAI")
		.WithTags("AI Agent")
		.Produces<AskAIResponse>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Ask AI a Question")
		.WithDescription("Sends a question to the AI agent and retrieves the answer.")
		.RequireAuthorization();
	}
}


