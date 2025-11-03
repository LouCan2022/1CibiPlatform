namespace PhilSys.Data.Repository;

public interface IPhilSysResultRepository
{
	Task<bool> AddTransactionResultDataAsync(PhilSysTransactionResult philSysTransactionResult);
}
