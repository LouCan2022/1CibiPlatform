namespace PhilSys.Services;

public class LivenessSessionService
{
	private readonly IPhilSysRepository _philSysRepository;

	public LivenessSessionService(IPhilSysRepository philSysRepository)
	{
		_philSysRepository = philSysRepository;
	}
	public async Task<TransactionStatusResponse> IsLivenessUsedAsync(string HashToken)
	{
		var status = await _philSysRepository.GetLivenessSessionStatusAsync(HashToken);
		
		return status;
	}
}
