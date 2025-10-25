namespace PhilSys.Services;
public class DeleteTransactionService
{
	private readonly IPhilSysRepository _philSysRepository;
	private readonly ILogger<DeleteTransactionService> _logger;

	public DeleteTransactionService(IPhilSysRepository philSysRepository,
									ILogger<DeleteTransactionService> logger)
	{
		_philSysRepository = philSysRepository;
		_logger = logger;
	}



	public async Task<bool> DeleteTransactionAsync(string HashToken)
	{

		var existingTransaction = await _philSysRepository.GetTransactionDataByHashTokenAsync(HashToken);

		if (existingTransaction == null)
		{
			_logger.LogWarning("Transaction with HashToken: {HashToken} not found.", HashToken);
			throw new Exception("No Transaction record found for this hashtoken.");
		}

		var deletedTransaction = await _philSysRepository.DeleteTrandsactionDataAsync(existingTransaction);

		return deletedTransaction;
	}
}
