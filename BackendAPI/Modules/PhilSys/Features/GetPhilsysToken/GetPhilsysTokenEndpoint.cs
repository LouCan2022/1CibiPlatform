namespace PhilSys.Features.GetPhilSysToken;
public record GetPhilSysTokenRequest(string client_id, string client_secret) : ICommand<GetPhilSysTokenResponse>;
public record GetPhilSysTokenResponse(CredentialResponseDTO CredentialResponseDTO);

public class GetPhilsysTokenEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("getphilsystoken", async (GetPhilSysTokenRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
		
			var command = new GetPhilSysTokenCommand(
				   request.client_id,
				   request.client_secret
			   );

			GetCredentialResult result = await sender.Send(command, cancellationToken);

			var response = new GetPhilSysTokenResponse(result.CredentialResponseDTO);

			return Results.Ok(response.CredentialResponseDTO);
		})
		.WithName("GetPhilSysToken")
		.WithTags("PhilSys")
		.Produces<GetPhilSysTokenResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Retrieve PhilSys Token")
		.WithDescription("Retrieves an access token from the PhilSys API using client credentials.");
	}
}
