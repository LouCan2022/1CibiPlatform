namespace PhilSys.Data.Repository;
public interface IPhilSysRepository
{
	Task<bool> AddTransactionDataAsync(PhilSysTransaction PhilSysTransaction);

	Task<PhilSysTransaction> UpdateTransactionDataAsync(PhilSysTransaction transaction);

	Task<PhilSysTransaction> UpdateFaceLivenessSessionAsync(string HashToken, string FaceLivenessSessionId);

	Task<PhilSysTransaction> GetTransactionDataByTidAsync(string HashToken);

	Task<TransactionStatusResponse> GetLivenessSessionStatusAsync(string HashToken);

	Task<bool> DeleteTrandsactionDataAsync(string HashToken);
}
