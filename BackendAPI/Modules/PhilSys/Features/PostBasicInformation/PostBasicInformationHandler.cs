namespace PhilSys.Features.PostBasicInformation;
public record PostBasicInformationCommand(string first_name,
										  string middle_name,
										  string last_name,
										  string suffix,
										  string birth_date,
										  string bearer_token,
										  string face_liveness_session_id) : ICommand<PostBasicInformationResult>;
public record PostBasicInformationResult(BasicInformationOrPCNResponseDTO BasicInformationResponseDTO);
public class PostBasicInformationHandler : ICommandHandler<PostBasicInformationCommand, PostBasicInformationResult>
{
	private readonly PostBasicInformationService _postBasicInformationService;
	public PostBasicInformationHandler(PostBasicInformationService PostBasicInformationService)
	{
		_postBasicInformationService = PostBasicInformationService;
	}
	public async Task<PostBasicInformationResult> Handle(PostBasicInformationCommand command, CancellationToken cancellationToken)
	{
		var result = await _postBasicInformationService.PostBasicInformationAsync(
				command.first_name,
				command.middle_name,
				command.last_name,
				command.suffix,
				command.birth_date,
				command.bearer_token,
				command.face_liveness_session_id
			);
		return new PostBasicInformationResult(result);
	}
}
