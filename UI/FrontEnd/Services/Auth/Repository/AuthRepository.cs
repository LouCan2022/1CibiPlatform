namespace FrontEnd.Services.Auth.Repository;

public class AuthRepository : IAuthRepository
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _configuration;
	private HttpClient _httpClient;

	public AuthRepository(IHttpClientFactory httpClientFactory,
		IHttpContextAccessor httpContextAccessor,
		IConfiguration configuration)
	{
		this._httpClient = httpClientFactory.CreateClient("OnePlatformAPI");
		this._httpContextAccessor = httpContextAccessor;
		this._configuration = configuration;
	}

	public async Task<string> Login(LoginCred cred)
	{
		var payload = new
		{
			loginCred = new
			{
				username = cred.Username,
				password = cred.Password
			}
		};

		var response = await _httpClient.PostAsJsonAsync("/login", payload);

		response.EnsureSuccessStatusCode();

		var token = await response.Content.ReadFromJsonAsync<CredResponse>();

		return token!.access_token;
	}

	public Task<string> ReadToken()
	{
		var key = _configuration.GetValue<string>("HttpCookieOnlyKey");

		// Read token from session
		var token = "0"; /*_httpContextAccessor.HttpContext?.Session.GetString(key!);*/

		return Task.FromResult(token ?? string.Empty);
	}
}

