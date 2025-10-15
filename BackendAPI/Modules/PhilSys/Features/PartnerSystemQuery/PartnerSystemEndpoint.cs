
namespace PhilSys.Features.PartnerSystemQuery;

public record PartnerSystemRequest(PartnerSystemRequestDTO PartnerSystemRequestDTO) : ICommand<PartnerSystemResponse>;	
public record PartnerSystemResponse(PartnerSystemResponseDTO PartnerSystemResponseDTO);

public class PartnerSystemEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("partnersystemquery", async (PartnerSystemRequest request, ISender sender, CancellationToken cancellationToken) =>
		{
			var command = new PartnerSystemCommand(request.PartnerSystemRequestDTO);

			PartnerSystemResult result = await sender.Send(command, cancellationToken);

			var response = new PartnerSystemResponse(result.PartnerSystemResponseDTO);

			return Results.Ok(response.PartnerSystemResponseDTO);
		})
		  .WithName("PartnerSystemQuery")
		  .WithTags("PhilSys")
		  .Produces<PartnerSystemResponse>()
		  .ProducesProblem(StatusCodes.Status400BadRequest)
		  .WithSummary("Retrieve If Verified")
		  .WithDescription("Retrieves an the verify response from the IDV endpoint using.");
	}
}
