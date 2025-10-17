namespace FrontendWebassembly.Services.PhilSys.Interfaces;

public interface IPhilSysService
{
	Task<Guid?> UpdateFaceLivenessSessionAsync(Guid Tid, string FaceLivenessSession);
}
