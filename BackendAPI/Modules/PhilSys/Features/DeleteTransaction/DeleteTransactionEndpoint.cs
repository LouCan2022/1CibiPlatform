namespace PhilSys.Features.DeleteTransaction;

public record DeleteTransactionRequest(string HashToken) : ICommand<DeleteTransactionResponse>;
public record DeleteTransactionResponse(bool IsDeleted);
public class DeleteTransactionEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("deletetransaction", async (DeleteTransactionCommand request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new DeleteTransactionCommand(
				request.HashToken
				);

			DeleteTransactionResult result = await sender.Send(command, cancellationToken);

			var response = new DeleteTransactionResponse(result.IsDeleted);

			return Results.Ok(response.IsDeleted);
		})
		.WithName("DeleteTransaction")
		.WithTags("PhilSys")
		.Produces<DeleteTransactionResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.ProducesProblem(StatusCodes.Status401Unauthorized)
		.WithSummary("Delete Transaction")
		.WithDescription("Delete Transaction");
	}
}
