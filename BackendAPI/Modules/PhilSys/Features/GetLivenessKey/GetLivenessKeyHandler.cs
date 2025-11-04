
namespace PhilSys.Features.GetLivenessKey;

public record GetLivenessKeyCommand() : ICommand<GetLivenessKeyResult>;

public record GetLivenessKeyResult(string LivenessKey);
public class GetLivenessKeyHandler : ICommandHandler<GetLivenessKeyCommand, GetLivenessKeyResult>
{
	private readonly GetLivenessKeyService _getLivenessKeyService;

	public GetLivenessKeyHandler(GetLivenessKeyService GetLivenessKeyService)
	{
		_getLivenessKeyService = GetLivenessKeyService;
	}
	public async Task<GetLivenessKeyResult> Handle(GetLivenessKeyCommand request, CancellationToken cancellationToken)
	{
		
		var livenessKey = await _getLivenessKeyService.GetLivenessKey();
		return new GetLivenessKeyResult(livenessKey);
	}
}
