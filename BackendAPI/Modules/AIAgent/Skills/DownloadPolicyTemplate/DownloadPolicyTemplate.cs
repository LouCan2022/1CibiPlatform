namespace AIAgent.Skills.DownloadPolicyTemplate;

public class DownloadPolicyTemplate : ISkill
{

	private readonly ILogger<DownloadPolicyTemplate> _logger;
	private readonly IPolicyTemplateFileStorageService _policyTemplateFileStorageService;

	public DownloadPolicyTemplate(
		ILogger<DownloadPolicyTemplate> logger,
		IPolicyTemplateFileStorageService policyTemplateFileStorageService)
	{
		_logger = logger;
		_policyTemplateFileStorageService = policyTemplateFileStorageService;
	}

	public async Task<object?> RunAsync(
		JsonElement payload,
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("Running DownloadPolicyTemplate skill");

		var templateUrl = await _policyTemplateFileStorageService.GetPolicyTemplateFileUrl();

		if (templateUrl == null)
		{
			throw new ArgumentNullException(nameof(templateUrl), "Policy template is not available.");
		}

		_logger.LogInformation("Retrieved policy template URL: {TemplateUrl}", templateUrl);

		return new
		{
			Message = "Template is ready for download.",
			DownloadUrl = templateUrl
		};
	}
}
