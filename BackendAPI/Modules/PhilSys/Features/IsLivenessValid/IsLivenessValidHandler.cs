
namespace PhilSys.Features.IsLivenessValid;

public record IsLivenessValidCommand(string HashToken) : ICommand<IsLivenessValidResult>;

public record IsLivenessValidResult(TransactionStatusResponse TransactionStatusResponse);
public class IsLivenessValidHandler : ICommandHandler<IsLivenessValidCommand, IsLivenessValidResult>
{
	private readonly ILogger<IsLivenessValidHandler> _logger;
	private readonly LivenessSessionService _livenessSessionService;

	public IsLivenessValidHandler(ILogger<IsLivenessValidHandler> logger,
		LivenessSessionService livenessSessionService)
	{
		_logger = logger;
		_livenessSessionService = livenessSessionService;
	}
	public async Task<IsLivenessValidResult> Handle(IsLivenessValidCommand request, CancellationToken cancellationToken)
	{
		var isLivenessValid = await _livenessSessionService.IsLivenessUsedAsync(request.HashToken);

		return new IsLivenessValidResult(isLivenessValid);
	}
}
