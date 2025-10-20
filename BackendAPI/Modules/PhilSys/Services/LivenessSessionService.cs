namespace PhilSys.Services;

public class LivenessSessionService
{
	private readonly IPhilSysRepository _philSysRepository;

	public LivenessSessionService(IPhilSysRepository philSysRepository)
	{
		_philSysRepository = philSysRepository;
	}
	public async Task<TransactionStatusResponse> IsLivenessUsedAsync(Guid Tid)
	{
		var status = await _philSysRepository.GetLivenessSessionStatus(Tid);
		if (status == null)
		{
			return new TransactionStatusResponse{ };
		}
		return status;
	}
}
