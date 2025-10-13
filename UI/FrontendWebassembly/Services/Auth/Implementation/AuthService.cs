namespace FrontendWebassembly.Services.Auth.Implementation;

public class AuthService : IAuthService
{
	private HttpClient _httpClient;
	private readonly LocalStorageService _localStorageService;
	private readonly IConfiguration _configuration;

	private readonly string _httpRefreshTokenCookieOnly;
	private readonly string _userNameKey;
	private readonly string _userIdKey;
	private readonly string _appIdKey;
	private readonly string _subMenuKey;
	private readonly string _roleIdKey;

	public AuthService(HttpClient httpClient,
		LocalStorageService localStorageService,
		IConfiguration configuration)
	{
		this._httpClient = httpClient;
		this._localStorageService = localStorageService;
		this._configuration = configuration;

		_httpRefreshTokenCookieOnly = _configuration.GetSection("AuthWeb:AuthWebHttpCookieOnlyKey").Value! ?? "";
		this._userNameKey = "Username";
		this._userIdKey = "UserId";
		this._appIdKey = "AppId";
		this._subMenuKey = "SubMenuId";
		this._roleIdKey = "RoleId";
	}

	public Task<string> GetUserInfoIfAuthenticated()
	{
		return Task.FromResult("sample");
	}

	public async Task<AuthResponseDTO> Login(LoginCred cred)
	{
		Console.WriteLine("🔹 Starting login request...");

		var payload = new
		{
			loginWebCred = new
			{
				Username = cred.Username,
				Password = cred.Password,
				IsRememberMe = cred.IsRememberMe
			}
		};

		Console.WriteLine($"➡️ Sending POST to /token/generatetoken for user: {cred.Username}");

		var response = await _httpClient.PostAsJsonAsync("/token/web/generatetoken", payload);
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

		Console.WriteLine("💾 Storing user info in local storage...");
		this.SetLocalstorage(successContent!);

		Console.WriteLine($"🎉 User {cred.Username} logged in successfully with UserId: {successContent!.UserId}");

		return new AuthResponseDTO(successContent.UserId, successContent.AccessToken, string.Empty, string.Empty);
	}

	protected virtual async void SetLocalstorage(CredResponseDTO credResponseDTO)
	{
		// Store UserId and Username in local storage
		await this._localStorageService.SetItemAsync(_userIdKey, credResponseDTO.UserId.ToString());
		await this._localStorageService.SetItemAsync(_userNameKey, credResponseDTO.UserName);
		await this._localStorageService.SetItemAsync(_appIdKey, JsonSerializer.Serialize(credResponseDTO.Appid));
		await this._localStorageService.SetItemAsync(_subMenuKey, JsonSerializer.Serialize(credResponseDTO.SubMenuid));
		await this._localStorageService.SetItemAsync(_roleIdKey, JsonSerializer.Serialize(credResponseDTO.RoleId));
	}

	public async Task<bool> IsAuthenticated()
	{
		var response = await _httpClient.GetAsync("/auth/isAuthenticated");

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("Something went wrong call the IT Team for further support {response}", response);
			return false;
		}

		var successContent = await response.Content.ReadFromJsonAsync<IsAuthenticatedDTO>();

		if (successContent!.isAuthenticated == false)
		{
			Console.WriteLine("User is not authenticated");
			return false;

		}

		return true;
	}

	public async Task<bool> Logout()
	{
		Console.WriteLine("🔹 Starting logout request...");

		var userId = await _localStorageService.GetItemAsync<Guid>(_userIdKey);

		if (userId == Guid.Empty)
		{
			Console.WriteLine("UserId not found in local storage. Cannot proceed with logout.");
			return false;
		}

		var payload = new
		{
			logoutDTO = new
			{
				UserId = userId,
				RevokeReason = "User Logged out"
			}
		};

		var response = await _httpClient.PostAsJsonAsync("/auth/logout", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("Something went wrong call the IT Team for further support {response}", JsonSerializer.Serialize(response));
			return false;
		}

		var successContent = await response.Content.ReadFromJsonAsync<LogoutResponseDTO>();

		if (successContent!.isLoggedOut == false)
		{
			Console.WriteLine("User is not logged out");
			return false;
		}


		// Clear local storage
		await this._localStorageService.ClearAsync();

		Console.WriteLine("✅ Logout successful.");

		return true;
	}
}
