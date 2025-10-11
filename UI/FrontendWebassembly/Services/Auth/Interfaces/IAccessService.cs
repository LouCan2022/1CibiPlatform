namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IAccessService
{
	Task<bool> HasAccessAsync(int appId, int subMenuId);
}