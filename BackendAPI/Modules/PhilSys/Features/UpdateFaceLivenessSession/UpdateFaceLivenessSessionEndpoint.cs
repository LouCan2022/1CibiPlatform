using System.Text.Json;

namespace PhilSys.Features.PostFaceLivenessSession;

public record UpdateFaceLivenessSessionRequest(string HashToken, string FaceLivenessSessionId) : ICommand<UpdateFaceLivenessSessionResponse>;

public record UpdateFaceLivenessSessionResponse(VerificationResponseDTO VerificationResponseDTO);
public class UpdateFaceLivenessSessionEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("updatefacelivenesssession", async (UpdateFaceLivenessSessionRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new UpdateFaceLivenessSessionCommand(
				request.HashToken,
				request.FaceLivenessSessionId
				);
			UpdateFaceLivenessSessionResult result = await sender.Send(command, cancellationToken);

			var response = new UpdateFaceLivenessSessionResponse(result.VerificationResponseDTO);

			return Results.Json(response.VerificationResponseDTO,
									new JsonSerializerOptions
									{
										DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
									}
							   );
		})
		.WithName("UpdateFaceLivenessSession")
		.WithTags("PhilSys")
		.Produces<UpdateFaceLivenessSessionResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.ProducesProblem(StatusCodes.Status401Unauthorized)
		.WithSummary("Update Face Liveness Session Id");
	}
}
