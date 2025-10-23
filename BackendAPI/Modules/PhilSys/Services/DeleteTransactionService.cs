namespace PhilSys.Services;
public class DeleteTransactionService
{
	private readonly IPhilSysRepository _philSysRepository;

	public DeleteTransactionService(IPhilSysRepository philSysRepository)
	{
		_philSysRepository = philSysRepository;
	}

	public async Task<bool> DeleteTransactionAsync(string HashToken)
	{
		var deletedTransaction = await _philSysRepository.DeleteTrandsactionDataAsync(HashToken);

		return deletedTransaction;
	}
}
