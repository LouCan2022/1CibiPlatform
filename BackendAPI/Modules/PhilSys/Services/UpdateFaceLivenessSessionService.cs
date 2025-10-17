namespace PhilSys.Services;
public class UpdateFaceLivenessSessionService
{
	private readonly IPhilSysRepository _philSysRepository;
	private readonly ILogger<UpdateFaceLivenessSessionService> _logger;
	public UpdateFaceLivenessSessionService(
		IPhilSysRepository philSysRepository,
		ILogger<UpdateFaceLivenessSessionService> logger)
	{
		_philSysRepository = philSysRepository;
		_logger = logger;
	}
	public async Task<bool> UpdateFaceLivenessSessionAsync(
		Guid Tid,
		string FaceLivenessSessionId,
		CancellationToken ct = default
		)
	{
		_logger.LogInformation("Updating Face Liveness Session for Tid: {Tid}", Tid);

		var result = await _philSysRepository.UpdateFaceLivenessSessionAsync(Tid, FaceLivenessSessionId);
		if (!result)
		{
			_logger.LogInformation("wdd updated Face Liveness Session for Tid: {Tid}", Tid);
			return false;
		}

		_logger.LogInformation("Successfully updated Face Liveness Session for Tid: {Tid}", Tid);
		return result;
	}
}
