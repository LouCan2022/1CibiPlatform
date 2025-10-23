namespace PhilSys.Features.UpdateFaceLivenessSession;

public record UpdateFaceLivenessSessionCommand(string HashToken, string FaceLivenessSessionId) : ICommand<UpdateFaceLivenessSessionResult>;

public record UpdateFaceLivenessSessionResult(BasicInformationOrPCNResponseDTO BasicInformationOrPCNResponseDTO);
public class UpdateFaceLivenessSessionHandler : ICommandHandler<UpdateFaceLivenessSessionCommand, UpdateFaceLivenessSessionResult>
{
	private readonly UpdateFaceLivenessSessionService _updateFaceLivenessSessionService;
	private readonly ILogger<UpdateFaceLivenessSessionCommand> _logger;

	public UpdateFaceLivenessSessionHandler(UpdateFaceLivenessSessionService UpdateFaceLivenessSessionService, ILogger<UpdateFaceLivenessSessionCommand> logger)
	{
		_updateFaceLivenessSessionService = UpdateFaceLivenessSessionService;
		_logger = logger;
	}
	public async Task<UpdateFaceLivenessSessionResult> Handle(UpdateFaceLivenessSessionCommand command, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling Philsys basic information request for client: {FirstName}", command.HashToken);

		var result = await _updateFaceLivenessSessionService.UpdateFaceLivenessSessionAsync(
				command.HashToken,
				command.FaceLivenessSessionId,
				cancellationToken
			);

		_logger.LogInformation("Successfully retrieved the Response");

		return new UpdateFaceLivenessSessionResult(result);
	}
}
