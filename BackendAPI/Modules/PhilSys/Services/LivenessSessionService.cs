

namespace PhilSys.Services;

public class LivenessSessionService
{
	private readonly IPhilSysRepository _philSysRepository;
	private readonly IHashService _hashService;
	private readonly ILogger<LivenessSessionService> _logger;

	public LivenessSessionService(IPhilSysRepository philSysRepository,
								  IHashService hashService,
								  ILogger<LivenessSessionService> logger)
	{
		_philSysRepository = philSysRepository;
		_hashService = hashService;
		_logger = logger;
	}
	public async Task<TransactionStatusResponseDTO> IsLivenessUsedAsync(string HashToken)
	{
		var status = await _philSysRepository.GetLivenessSessionStatusAsync(HashToken);

		if (status == null)
		{
			_logger.LogWarning("There is no transaction for {Token}", HashToken);
			throw new Exception($"There is no such transaction for Token: {HashToken}");
		}

		var hashTokenChecker = await _philSysRepository.GetTransactionDataByHashTokenAsync(HashToken);

		if (hashTokenChecker == null)
		{
			_logger.LogWarning("There is no transaction for {Token}", HashToken);
			throw new Exception($"There is no such transaction for Token: {HashToken}");
		}

		var isTokenValid = _hashService.Verify(HashToken, hashTokenChecker.HashToken!);

		if (!isTokenValid)
		{
			_logger.LogWarning("Invalid Token Provided: {Token}", HashToken);
			throw new Exception("Invalid Token");
		}

		_logger.LogInformation("Successfully received the transaction record for {Token}.", HashToken);

		return status.Adapt<TransactionStatusResponseDTO>();
	}
}
