namespace PhilSys.Data.Repository;
public interface IPhilSysRepository
{
	Task<bool> AddTransactionDataAsync(PhilSysTransaction PhilSysTransaction);

	Task<PhilSysTransaction> UpdateTransactionDataAsync(Guid Tid);

	Task<PhilSysTransaction> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSessionId);

	Task<PhilSysTransaction> GetTransactionDataByTidAsync(Guid Tid);

	Task<TransactionStatusResponse> GetLivenessSessionStatusAsync(Guid Tid);

	Task<bool> DeleteTrandsactionDataAsync(Guid Tid);
}
