namespace PhilSys.Services;

public class GetLivenessKeyService
{
	private readonly ILogger<GetLivenessKeyService> _logger;
	private readonly HybridCache hyrbridCache;
	private readonly IConfiguration _configuration;
	private string _livenessKey => _configuration["PhilSys:LivenessSDKPublicKey"] ?? "";

	public GetLivenessKeyService(ILogger<GetLivenessKeyService> logger, HybridCache hyrbridCache, IConfiguration configuration)
	{
		_logger = logger;
		this.hyrbridCache = hyrbridCache;
		_configuration = configuration;
	}
	public ValueTask<string> GetLivenessKey()
	{
		var cacheKey = "PhilSys_LivenessSDKPublicKey";
		var logContext = new
		{
			Action = "RetrievingLivenessKey",
			Step = "StartFetching",
			Timestamp = DateTime.UtcNow
		};

		_logger.LogInformation("Retrieving Liveness Key: {@Context}", logContext);
		return hyrbridCache.GetOrCreateAsync<string>(
			cacheKey,
			getKey =>
			{
				if (string.IsNullOrEmpty(_livenessKey))
				{
					_logger.LogError("Liveness Key is not configured: {@Context}", logContext);
					return new ValueTask<string>(string.Empty);
				}

				_logger.LogInformation("Liveness Key retrieved successfully: {@Context}", logContext);
				 return new ValueTask<string>(_livenessKey);
			});
	}
}
