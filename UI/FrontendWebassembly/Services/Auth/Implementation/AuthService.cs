namespace FrontendWebassembly.Services.Auth.Implementation;

public class AuthService : IAuthService
{
	private HttpClient _httpClient;
	private readonly IConfiguration _configuration;

	public AuthService(HttpClient httpClient,
		IConfiguration configuration)
	{
		this._httpClient = httpClient;
		this._configuration = configuration;
	}

	public Task<string> GetUserInfoIfAuthenticated()
	{
		return Task.FromResult(string.Empty);
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
}
