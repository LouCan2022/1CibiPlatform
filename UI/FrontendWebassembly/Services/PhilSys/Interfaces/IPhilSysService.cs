namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IPhilSysService
{
	Task<bool> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSession);
}
