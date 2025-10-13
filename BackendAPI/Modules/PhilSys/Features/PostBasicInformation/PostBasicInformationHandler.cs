
namespace PhilSys.Features.PostBasicInformation;

public record PostBasicInformationCommand(string first_name,
										  string middle_name,
										  string last_name,
										  string suffix,
										  DateTime birth_date,
										  string face_liveness_session_id) : ICommand<PostBasicInformationResponse>;
public record PostBasicInformationResponse(BasicInformationOrPCNResponseDTO BasicInformationResponseDTO);
public class PostBasicInformationHandler : ICommandHandler<PostBasicInformationCommand, PostBasicInformationResponse>
{
	private readonly PostBasicInformationService _postBasicInformationService;
	private readonly ILogger<PostBasicInformationHandler> _logger;

	public PostBasicInformationHandler(PostBasicInformationService PostBasicInformationService, ILogger<PostBasicInformationHandler> logger)
	{
		_postBasicInformationService = PostBasicInformationService;
		_logger = logger;
	}

	public async Task<PostBasicInformationResponse> Handle(PostBasicInformationCommand command, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling Philsys basic information request for client: {FirstName}", command.first_name);

		var tokenResult = await _postBasicInformationService.PostBasicInformationAsync(
				command.first_name,
				command.middle_name,
				command.last_name,
				command.suffix,
				command.birth_date,
				command.face_liveness_session_id
			);

		_logger.LogInformation("Successfully retrieved the Response");

		return new PostBasicInformationResponse(tokenResult);
	}
}
