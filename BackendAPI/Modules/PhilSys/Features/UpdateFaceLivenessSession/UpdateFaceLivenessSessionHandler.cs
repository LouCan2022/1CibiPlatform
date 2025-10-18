namespace PhilSys.Features.UpdateFaceLivenessSession;

public record UpdateFaceLivenessSessionCommand(Guid Tid, string FaceLivenessSessionId) : ICommand<UpdateFaceLivenessSessionResult>;

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
		_logger.LogInformation("Handling Philsys basic information request for client: {FirstName}", command.Tid);

		var result = await _updateFaceLivenessSessionService.UpdateFaceLivenessSessionAsync(
				command.Tid,
				command.FaceLivenessSessionId,
				cancellationToken
			);

		_logger.LogInformation("Successfully retrieved the Response");

		return new UpdateFaceLivenessSessionResult(result);
	}
}
