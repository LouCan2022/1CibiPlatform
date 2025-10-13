using PhilSys.Features.GetPhilSysToken;

namespace PhilSys.Features.PostBasicInformation;

public record PostBasicInformationRequest(string first_name,
										  string middle_name,
										  string last_name,
										  string suffix,
										  DateTime birth_date,
										  string face_liveness_session_id) : ICommand<PostBasicInformationResponse>;

public record PostBasicInformationResponse(BasicInformationOrPCNResponseDTO BasicInformationResponseDTO);
public class PostBasicInformationEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("PostBasicInformation", async (PostBasicInformationRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new PostBasicInformationCommand(
				request.first_name,
				request.middle_name,
				request.last_name,
				request.suffix,
				request.birth_date,
				request.face_liveness_session_id
				);

			PostBasicInformationResult result = await sender.Send(command, cancellationToken);

			var response = new PostBasicInformationResponse(result.BasicInformationResponseDTO);

			return Results.Ok(response.BasicInformationResponseDTO);
		})
		.WithName("PostBasicInformation")
		.WithTags("PhilSys")
		.Produces<GetPhilSysTokenResponse>()
		.ProducesProblem(StatusCodes.Status400BadRequest)
		.WithSummary("Retrieve If Verified")
		.WithDescription("Retrieves an the verify response from the PhilSys API using client credentials.");

	}
}
