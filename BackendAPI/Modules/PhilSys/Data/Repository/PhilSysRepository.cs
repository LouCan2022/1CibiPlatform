
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
	public async Task<PhilSysTransaction> UpdateTransactionDataAsync(PhilSysTransaction transaction)
	{
		var entry = _dbcontext.Attach(transaction);
		entry.Property(t => t.IsTransacted).IsModified = true;
		entry.Property(t => t.TransactedAt).IsModified = true;

		transaction!.IsTransacted = true;
		transaction.TransactedAt = DateTime.UtcNow;

		await _dbcontext.SaveChangesAsync();

		return transaction;
	}

	public async Task<PhilSysTransaction> GetTransactionDataByHashTokenAsync(string HashToken)
	{
		var transaction = await _dbcontext.PhilSysTransactions.FirstOrDefaultAsync(x => x.HashToken == HashToken);

		return transaction!;
	}

	public async Task<PhilSysTransaction> UpdateFaceLivenessSessionAsync(string HashToken, string FaceLivenessSessionId)
	{
		var transaction = await _dbcontext.PhilSysTransactions.FirstOrDefaultAsync(x => x.HashToken == HashToken);

		transaction!.FaceLivenessSessionId = FaceLivenessSessionId;

		await _dbcontext.SaveChangesAsync();

		return transaction;
	}

	public async Task<TransactionStatusResponse> GetLivenessSessionStatusAsync(string HashToken)
	{
		var transaction = await _dbcontext.PhilSysTransactions
		.AsNoTracking()
		.Where(t => t.HashToken == HashToken)
		.Select(t => new TransactionStatusResponse
		{
			Exists = true, 
			IsTransacted = t.IsTransacted,
			isExpired = false,
			ExpiresAt = t.ExpiresAt
		})
		.FirstOrDefaultAsync();


		return transaction ?? new TransactionStatusResponse { Exists = false };
	}

	public async Task<bool> DeleteTrandsactionDataAsync(PhilSysTransaction transaction)
	{
		_dbcontext.PhilSysTransactions.Remove(transaction!);
		await _dbcontext.SaveChangesAsync();
		return true;
	}
}
