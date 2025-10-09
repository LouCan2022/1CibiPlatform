namespace PhilSys.Features.GetPhilSysToken;
public record GetPhilSysTokenCommand(string client_id, string client_secret) : ICommand<GetCredentialResult>;
public record GetCredentialResult(CredentialResponseDTO CredentialResponseDTO);

public class GetPhilSysTokenHandler : ICommandHandler<GetPhilSysTokenCommand, GetCredentialResult>
{
	private readonly GetTokenService _getTokenService;
	private readonly ILogger<GetPhilSysTokenHandler> _logger;

	public GetPhilSysTokenHandler(GetTokenService getTokenService, ILogger<GetPhilSysTokenHandler> logger)
	{
		_getTokenService = getTokenService;
		_logger = logger;
	}

	public async Task<GetCredentialResult> Handle(GetPhilSysTokenCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling PhilSys token request for client: {ClientId}", request.client_id);

		var tokenResult = await _getTokenService.GetPhilsysTokenAsync(
			request.client_id,
			request.client_secret,
			cancellationToken
		);

		_logger.LogInformation("Successfully retrieved PhilSys token");

		return new GetCredentialResult(tokenResult);
	}
}
