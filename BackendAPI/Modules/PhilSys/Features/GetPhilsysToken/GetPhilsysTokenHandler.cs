namespace PhilSys.Features.GetPhilSysToken;
public record GetPhilSysTokenCommand(string client_id, string client_secret) : ICommand<GetCredentialResult>;
public record GetCredentialResult(string AccessToken);
public class GetPhilSysTokenHandler : ICommandHandler<GetPhilSysTokenCommand, GetCredentialResult>
{
	private readonly IPhilSysService _philsyService;
	public GetPhilSysTokenHandler(IPhilSysService philSysService)
	{
		_philsyService = philSysService;
	}
	public async Task<GetCredentialResult> Handle(GetPhilSysTokenCommand command, CancellationToken cancellationToken)
	{
		var tokenResult = await _philsyService.GetPhilsysTokenAsync(
			command.client_id,
			command.client_secret
		);
		return new GetCredentialResult(tokenResult);
	}
}
