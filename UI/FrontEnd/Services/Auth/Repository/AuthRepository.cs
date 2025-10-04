namespace FrontEnd.Services.Auth.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private HttpClient _httpClient;

    public AuthRepository(IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        this._httpClient = httpClientFactory.CreateClient("OnePlatformAPI");
        this._httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> IsTokenExist()
    {
        var token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
        return !string.IsNullOrEmpty(token);
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

    public Task<bool> SetToken(string token)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Response == null)
            return Task.FromResult(false);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        httpContext.Response.Cookies.Append("AuthToken", token, cookieOptions);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        httpContext!.Response.Cookies.Delete("AuthToken");
        return Task.FromResult(true);
    }

}

