using FrontendWebassembly.DTO.Auth;

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

	public async Task<AuthResponseDTO> Login(LoginCred cred)
	{
		Console.WriteLine("🔹 Starting login request...");

		var payload = new
		{
			loginCred = new
			{
				username = cred.Username,
				password = cred.Password
			}
		};

		Console.WriteLine($"➡️ Sending POST to /token/generatetoken for user: {cred.Username}");

		var response = await _httpClient.PostAsJsonAsync("/token/generatetoken", payload);
		Console.WriteLine($"⬅️ Received response: {(int)response.StatusCode} {response.ReasonPhrase}");

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Login failed. Reading error content...");

			var errorContent = await response.Content.ReadFromJsonAsync<CredErrorResponseDTO>();

			var rawError = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"⚠️ Could not parse JSON error. Raw content: {rawError}");
			return new AuthResponseDTO(Guid.Empty, string.Empty, rawError, "Unknown Error");
		}

		Console.WriteLine("✅ Login successful. Reading success content...");

		var successContent = await response.Content.ReadFromJsonAsync<CredResponseDTO>();

		Console.WriteLine($"🎉 User {cred.Username} logged in successfully with UserId: {successContent.UserId}");

		return new AuthResponseDTO(successContent.UserId, successContent.AccessToken, string.Empty, string.Empty);
	}

}
