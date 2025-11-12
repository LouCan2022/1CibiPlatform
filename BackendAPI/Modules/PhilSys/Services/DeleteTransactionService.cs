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
			throw new NotFoundException("Transaction record not found.");
		}

		var deletedTransaction = await _philSysRepository.DeleteTransactionDataAsync(existingTransaction);

		if (deletedTransaction == false)
		{
			_logger.LogError("Failed to delete Transaction record for HashToken: {HashToken}", HashToken);
			throw new Exception("Failed to delete the transaction record.");
		}

		_logger.LogInformation("Successfully Deleted the Transaction record for {HashToken}.", HashToken);

		return true;
	}
}
