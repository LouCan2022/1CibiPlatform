
namespace PhilSys.Data.Repository;

public class PhilSysResultRepository : IPhilSysResultRepository
{
	private readonly PhilSysDBContext _dbcontext;

	public PhilSysResultRepository(PhilSysDBContext dbcontext)
	{
		_dbcontext = dbcontext;
	}
	public async Task<bool> AddTransactionResultDataAsync(PhilSysTransactionResult philSysTransactionResult)
	{
		await _dbcontext.PhilSysTransactionResults.AddAsync(philSysTransactionResult);
		await _dbcontext.SaveChangesAsync();
		return true;
	}
}
