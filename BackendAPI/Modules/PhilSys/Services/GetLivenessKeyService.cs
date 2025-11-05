namespace PhilSys.Services;

public class GetLivenessKeyService
{
	private readonly ILogger<GetLivenessKeyService> _logger;
	private readonly IConfiguration _configuration;
	private string _livenessKey => _configuration["PhilSys:LivenessSDKPublicKey"] ?? "";

	public GetLivenessKeyService(ILogger<GetLivenessKeyService> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}
	public Task<string> GetLivenessKey()
	{
		_logger.LogInformation("Retrieving Liveness Key.");
		if (string.IsNullOrEmpty(_livenessKey))
		{
			_logger.LogError("Liveness Key is not configured.");
			return Task.FromResult(string.Empty);
		}
		_logger.LogInformation("Liveness Key retrieved successfully.");
		return Task.FromResult(_livenessKey);
	}
}
