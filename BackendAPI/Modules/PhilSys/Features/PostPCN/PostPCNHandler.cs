namespace PhilSys.Features.PostPCN;

public record PostPCNCommand(int value,
							 string face_liveness_session_id) : ICommand<PostPCNResponse>;
public record PostPCNResponse(BasicInformationOrPCNResponseDTO PCNResponseDTO);
public class PostPCNHandler : ICommandHandler<PostPCNCommand, PostPCNResponse>
{
	private readonly PostPCNService _postPCNService;
	private readonly ILogger<PostPCNHandler> _logger;

	public PostPCNHandler(PostPCNService postPCNService, ILogger<PostPCNHandler> logger)
	{
		_postPCNService = postPCNService;
		_logger = logger;
	}

	public async Task<PostPCNResponse> Handle(PostPCNCommand command, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling Philsys basic information request for client: {FirstName}", command.value);

		var tokenResult = await _postPCNService.PostBasicInformationAsync(
				command.value,
				command.face_liveness_session_id,
				cancellationToken
			);

		_logger.LogInformation("Successfully retrieved the Response");

		return new PostPCNResponse(tokenResult);
	}
}
