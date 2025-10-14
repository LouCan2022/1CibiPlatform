namespace PhilSys.Data.Repository;
public interface IPhilSysRepository
{
	Task<bool> AddTransactionDataAsync(PhilSysTransaction PhilSysTransaction);

	Task<bool> UpdateTransactionDataAsync(Guid Tid, PhilSysTransaction PhilSysTransaction);

	Task<PhilSysTransaction> GetTransactionDataByTidAsync(Guid Tid);
}
