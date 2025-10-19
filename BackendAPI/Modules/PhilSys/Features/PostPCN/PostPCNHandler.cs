namespace PhilSys.Features.PostPCN;

public record PostPCNCommand(string value,
							 string bearer_token,	
							 string face_liveness_session_id) : ICommand<PostPCNResult>;
public record PostPCNResult(BasicInformationOrPCNResponseDTO PCNResponseDTO);
public class PostPCNHandler : ICommandHandler<PostPCNCommand, PostPCNResult>
{
	private readonly PostPCNService _postPCNService;
	private readonly ILogger<PostPCNHandler> _logger;

	public PostPCNHandler(PostPCNService postPCNService, ILogger<PostPCNHandler> logger)
	{
		_postPCNService = postPCNService;
		_logger = logger;
	}

	public async Task<PostPCNResult> Handle(PostPCNCommand command, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling Philsys basic information request for client: {FirstName}", command.value);

		var result = await _postPCNService.PostPCNAsync(
				command.value,
				command.bearer_token,
				command.face_liveness_session_id,
				cancellationToken
			);

		_logger.LogInformation("Successfully retrieved the Response");

		return new PostPCNResult(result);
	}
}
