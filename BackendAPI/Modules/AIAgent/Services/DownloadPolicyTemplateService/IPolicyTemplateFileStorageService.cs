namespace AIAgent.Services.DownloadPolicyTemplate;

public interface IPolicyTemplateFileStorageService
{
	Task<string> GetPolicyTemplateFileUrl();
}
