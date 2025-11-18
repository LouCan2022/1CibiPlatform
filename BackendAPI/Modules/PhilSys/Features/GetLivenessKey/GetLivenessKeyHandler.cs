namespace PhilSys.Features.GetLivenessKey;

public record GetLivenessKeyQueryRequest() : IRequest<GetLivenessKeyResult>;

public record GetLivenessKeyResult(string LivenessKey);
public class GetLivenessKeyHandler : IRequestHandler<GetLivenessKeyQueryRequest, GetLivenessKeyResult>
{
	private readonly GetLivenessKeyService _getLivenessKeyService;

	public GetLivenessKeyHandler(GetLivenessKeyService GetLivenessKeyService)
	{
		_getLivenessKeyService = GetLivenessKeyService;
	}
	public async Task<GetLivenessKeyResult> Handle(GetLivenessKeyQueryRequest request, CancellationToken cancellationToken)
	{
		var livenessKey = await _getLivenessKeyService.GetLivenessKey();
		return new GetLivenessKeyResult(livenessKey);
	}
}
