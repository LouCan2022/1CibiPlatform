
namespace PhilSys.Data.Repository;

public class PhilSysRepository : IPhilSysRepository
{
	private readonly PhilSysDBContext _dbcontext;

	public PhilSysRepository(PhilSysDBContext dbcontext)
	{
		_dbcontext = dbcontext;
	}
	public async Task<bool> AddTransactionDataAsync(PhilSysTransaction PhilSysTransaction)
	{
		await _dbcontext.PhilSysTransactions.AddAsync(PhilSysTransaction);
		await _dbcontext.SaveChangesAsync();
		return true;
	}
	public async Task<PhilSysTransaction> UpdateTransactionDataAsync(Guid Tid, PhilSysTransaction PhilSysTransaction)
	{
		var transaction = await _dbcontext.PhilSysTransactions.FirstOrDefaultAsync(x => x.Tid == Tid);
	
		transaction!.IsTransacted = true;
		transaction.TransactedAt = DateTime.UtcNow;

		await _dbcontext.SaveChangesAsync();

		return transaction;
	}

	public async Task<PhilSysTransaction> GetTransactionDataByTidAsync(Guid Tid)
	{
		var transaction = await _dbcontext.PhilSysTransactions.FirstOrDefaultAsync(x => x.Tid == Tid);

		return transaction!;
	}

	public async Task<bool> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSessionId)
	{
		var transaction = await _dbcontext.PhilSysTransactions.FirstOrDefaultAsync(x => x.Tid == Tid);

		transaction!.FaceLivenessSessionId = FaceLivenessSessionId;

		await _dbcontext.SaveChangesAsync();

		return true;
	}
}
