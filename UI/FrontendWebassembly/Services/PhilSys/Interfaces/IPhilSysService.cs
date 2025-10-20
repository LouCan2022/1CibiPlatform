namespace FrontendWebassembly.Services.PhilSys.Interfaces;

public interface IPhilSysService
{
	Task<UpdateFaceLivenessSessionResponseDTO> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSession);

	Task<TransactionStatusResponse> GetTransactionStatus (Guid Tid);

}
