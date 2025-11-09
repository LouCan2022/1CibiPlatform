namespace PhilSys.Features.GetPhilSysToken;
public record GetPhilSysTokenCommand(string client_id, string client_secret) : ICommand<GetCredentialResult>;
public record GetCredentialResult(string AccessToken);
public class GetPhilSysTokenHandler : ICommandHandler<GetPhilSysTokenCommand, GetCredentialResult>
{
	private readonly GetTokenService _getTokenService;
	public GetPhilSysTokenHandler(GetTokenService getTokenService)
	{
		_getTokenService = getTokenService;
	}
	public async Task<GetCredentialResult> Handle(GetPhilSysTokenCommand command, CancellationToken cancellationToken)
	{
		var tokenResult = await _getTokenService.GetPhilsysTokenAsync(
			command.client_id,
			command.client_secret
		);
		return new GetCredentialResult(tokenResult);
	}
}
