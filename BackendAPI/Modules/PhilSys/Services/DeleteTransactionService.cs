namespace PhilSys.Services;
public class DeleteTransactionService
{
	private readonly IPhilSysRepository _philSysRepository;

	public DeleteTransactionService(IPhilSysRepository philSysRepository)
	{
		_philSysRepository = philSysRepository;
	}

	public async Task<bool> DeleteTransactionAsync(Guid Tid)
	{
		var deletedTransaction = await _philSysRepository.DeleteTrandsactionDataAsync(Tid);

		return deletedTransaction;
	}
}
