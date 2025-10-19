namespace PhilSys.Data.Repository;
public interface IPhilSysRepository
{
	Task<bool> AddTransactionDataAsync(PhilSysTransaction PhilSysTransaction);

	Task<PhilSysTransaction> UpdateTransactionDataAsync(Guid Tid, PhilSysTransaction PhilSysTransaction);

	Task<PhilSysTransaction> GetTransactionDataByTidAsync(Guid Tid);

	Task<PhilSysTransaction> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSessionId);
}
