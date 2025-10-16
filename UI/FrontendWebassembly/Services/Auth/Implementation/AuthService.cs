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
				Username = cred.Email,
				Password = cred.Password,
				IsRememberMe = cred.IsRememberMe
			}
		};

		Console.WriteLine($"➡️ Sending POST to /token/generatetoken for user: {cred.Email}");

		var response = await _httpClient.PostAsJsonAsync("/token/web/generatetoken", payload);
		Console.WriteLine($"⬅️ Received response: {(int)response.StatusCode} {response.ReasonPhrase}");

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Login failed. Reading error content...");

			var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

			Console.WriteLine($"⚠️ Could not parse JSON error. Raw content: {errorContent!.Detail}");
			return new AuthResponseDTO(Guid.Empty, string.Empty, errorContent.Detail, "Error");
		}

		Console.WriteLine("✅ Login successful. Reading success content...");

		var successContent = await response.Content.ReadFromJsonAsync<CredResponseDTO>();

		Console.WriteLine("💾 Storing user info in local storage...");
		this.SetLocalstorage(successContent!);

		Console.WriteLine($"🎉 User {cred.Email} logged in successfully with UserId: {successContent!.UserId}");

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

	public async Task<RegisterResponseDTO> Register(RegisterRequestDTO registerRequestDTO)
	{
		Console.WriteLine("🔹 Starting registration request...");

		var payload = new
		{
			register = new
			{
				email = registerRequestDTO.Email,
				passwordHash = registerRequestDTO.PasswordHash,
				firstName = registerRequestDTO.FirstName,
				lastName = registerRequestDTO.LastName,
				middleName = registerRequestDTO.MiddleName
			}
		};

		Console.WriteLine($"➡️ Sending POST to /auth/register for email: {registerRequestDTO.Email}");

		var response = await _httpClient.PostAsJsonAsync("/auth/register", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Registration failed. Reading error content...");

			var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

			return new RegisterResponseDTO(Guid.Empty, string.Empty, errorContent!.Detail);
		}

		Console.WriteLine("✅ Registration successful. Reading success content...");

		var successContent = await response.Content.ReadFromJsonAsync<OtpVerificationResponseDTO>();

		return new RegisterResponseDTO(successContent!.OtpId, successContent!.Email, string.Empty);
	}

	public async Task<OtpSessionResponseDTO> IsOtpSessionValid(OtpSessionRequestDTO otpRequestDTO)
	{

		Console.WriteLine("🔹 Starting OTP validation request...");

		var payload = new
		{
			OtpVerificationRequestDTO = new
			{
				userId = otpRequestDTO.userId,
				email = otpRequestDTO.email
			}
		};

		Console.WriteLine($"➡️ Sending POST to /auth/validate/otp for UserId: {otpRequestDTO.userId}, Email: {otpRequestDTO.email}");

		var response = await _httpClient.PostAsJsonAsync("/auth/validate/otp", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ OTP validation failed. Reading error content...");

			var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

			return new OtpSessionResponseDTO(false, errorContent!.Detail);
		}

		Console.WriteLine("✅ OTP validation successful. Reading success content...");

		var successContent = await response.Content.ReadFromJsonAsync<OtpVerificationSessionResponseDTO>();

		return new OtpSessionResponseDTO(successContent!.isOtpSessionValid, string.Empty);
	}

	public async Task<OtpSessionResponseDTO> OtpVerification(OtpVerificationRequestDTO otpVerificationRequestDTO)
	{
		Console.WriteLine("🔹 Starting OTP verification request...");

		var payload = new
		{
			OtpRequestDTO = new
			{
				Email = otpVerificationRequestDTO.Email,
				Otp = otpVerificationRequestDTO.Otp,
			}
		};

		Console.WriteLine($"➡️ Sending POST to /auth/verify/otp for Email: {otpVerificationRequestDTO.Email}");

		var response = await _httpClient.PostAsJsonAsync("/auth/verify/otp", payload);

		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ OTP verification failed. Reading error content...");
			var errorContent = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
			return new OtpSessionResponseDTO(false, errorContent!.Detail);
		}

		Console.WriteLine("✅ OTP verification successful.");

		return new OtpSessionResponseDTO(true, string.Empty);


	}
}
