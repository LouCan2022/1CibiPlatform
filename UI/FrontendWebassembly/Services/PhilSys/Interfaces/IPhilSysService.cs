namespace FrontendWebassembly.Services.PhilSys.Interfaces;

public interface IPhilSysService
{
	Task<UpdateFaceLivenessSessionResponseDTO> UpdateFaceLivenessSessionAsync(string HashToken, string FaceLivenessSession);

	Task<TransactionStatusResponse> GetTransactionStatusAsync(string HashToken);

	Task<bool> DeleteTransactionAsync(string HashToken);

	Task<string> PostBasicInformationOrPCN(string inquiry_type, IdentityData identity_data);

}
