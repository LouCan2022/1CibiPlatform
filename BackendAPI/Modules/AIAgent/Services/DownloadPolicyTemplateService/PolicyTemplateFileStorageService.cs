namespace AIAgent.Services.DownloadPolicyTemplate;


public class PolicyTemplateFileStorageService : IPolicyTemplateFileStorageService
{

	private readonly ILogger<FileStorageService> _logger;
	private const string BaseUrl = "/api/files";
	private readonly string filename = "PolicyTemplate.xlsx";

	public PolicyTemplateFileStorageService(ILogger<FileStorageService> logger, IConfiguration configuration)
	{
		_logger = logger;
	}

	public async Task<string> GetPolicyTemplateFileUrl()
	{
		return $"{BaseUrl}/{filename}";
	}
}
