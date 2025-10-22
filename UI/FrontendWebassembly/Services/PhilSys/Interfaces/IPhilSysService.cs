namespace FrontendWebassembly.Services.PhilSys.Interfaces;

public interface IPhilSysService
{
	Task<UpdateFaceLivenessSessionResponseDTO> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSession);

	Task<TransactionStatusResponse> GetTransactionStatusAsync(Guid Tid);

	Task<bool> DeleteTransactionAsync(Guid Tid);

}
