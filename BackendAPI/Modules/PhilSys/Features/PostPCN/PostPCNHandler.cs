namespace PhilSys.Features.PostPCN;
public record PostPCNCommand(string value,
							 string bearer_token,	
							 string face_liveness_session_id) : ICommand<PostPCNResult>;
public record PostPCNResult(BasicInformationOrPCNResponseDTO PCNResponseDTO);
public class PostPCNHandler : ICommandHandler<PostPCNCommand, PostPCNResult>
{
	private readonly PostPCNService _postPCNService;
	public PostPCNHandler(PostPCNService postPCNService)
	{
		_postPCNService = postPCNService;
	}
	public async Task<PostPCNResult> Handle(PostPCNCommand command, CancellationToken cancellationToken)
	{
		var result = await _postPCNService.PostPCNAsync(
				command.value,
				command.bearer_token,
				command.face_liveness_session_id,
				cancellationToken
			);
		return new PostPCNResult(result);
	}
}
