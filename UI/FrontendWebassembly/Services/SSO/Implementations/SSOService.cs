namespace FrontendWebassembly.Services.SSO.Implementations;

public class SSOService : ISSOService
{
	private HttpClient _httpClient;
	private readonly LocalStorageService _localStorageService;
	private readonly IConfiguration _configuration;

	public SSOService(IHttpClientFactory httpClientFactory,
		LocalStorageService localStorageService,
		IConfiguration configuration)
	{
		this._httpClient = httpClientFactory.CreateClient("SSOAPI");
		this._localStorageService = localStorageService;
		this._configuration = configuration;
	}

	public async Task<bool> IsUserAuthenticatedAsync()
	{
		Console.WriteLine("🔹 Starting SSO authentication check...");

		var response = await _httpClient.GetAsync("/sso/is-user-authenticated");

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine($"❌ SSO authentication check failed. Response: {response}");
			return false;
		}

		var successContent = await response.Content.ReadFromJsonAsync<IsUserAuthenticatedResponse>();

		Console.WriteLine($"🔹 SSO authentication check response received. {successContent}");

		if (successContent?.isAuthenticated == false)
		{
			Console.WriteLine("❌ User is not authenticated via SSO");
			return false;
		}

		Console.WriteLine("✅ User is authenticated via SSO");

		return true;
	}

	public async Task<bool> LogoutAsync()
	{
		Console.WriteLine("🔹 Logging out via SSO...");

		var response = await _httpClient.PostAsync("/sso/logout", null);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine($"❌ SSO logout failed. Response: {response}");
			return false;
		}

		Console.WriteLine("✅ SSO logout successful.");
		return true;
	}
}
