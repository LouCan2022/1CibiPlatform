
namespace PhilSys.Data.Repository;

public class PhilSysRepository : IPhilSysRepository
{
	private readonly PhilSysDBContext dbcontext;

	public PhilSysRepository(PhilSysDBContext dbcontext)
	{
		this.dbcontext = dbcontext;
	}
	public async Task<bool> AddTransactionDataAsync(PhilSysTransaction PhilSysTransaction)
	{
		await dbcontext.PhilSysTransactions.AddAsync(PhilSysTransaction);
		await dbcontext.SaveChangesAsync();
		return true;
	}
	public async Task<bool> UpdateTransactionDataAsync(Guid Tid, PhilSysTransaction PhilSysTransaction)
	{
		var transaction = dbcontext.PhilSysTransactions.FirstOrDefault(x => x.Tid == Tid);
		if (transaction == null)
			return false;

		transaction.IsTransacted = true;
		transaction.TransactedAt = DateTime.UtcNow;

		await dbcontext.SaveChangesAsync();

		return true;
	}

	public async Task<PhilSysTransaction> GetTransactionDataByTidAsync(Guid Tid)
	{
		var transaction = await dbcontext.PhilSysTransactions.FirstOrDefaultAsync(x => x.Tid == Tid);

		if (transaction == null)
			return null!;

		return transaction;
	}

}
