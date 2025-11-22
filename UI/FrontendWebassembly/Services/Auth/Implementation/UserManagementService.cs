namespace FrontendWebassembly.Services.Auth.Implementation;

public class UserManagementService : IUserManagementService
{
	private readonly HttpClient _httpClient;

	public UserManagementService(IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClientFactory.CreateClient("API");
	}

	public async Task<PaginatedResult<UsersDTO>> GetUsersAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken ct = default)
	{
		// Build query string
		var query = $"auth/getusers?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		// Make HTTP request
		var response = await _httpClient.GetFromJsonAsync<UsersResponseDTO>(query, ct);

		// Handle null response
		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the Users Successfully");

			// Return an empty PaginatedResult
			return new PaginatedResult<UsersDTO>(
				pageIndex: PageNumber ?? 1,
				pageSize: PageSize ?? 10,
				count: 0,
				data: Enumerable.Empty<UsersDTO>()
			);
		}

		return response.users!;
	}

	public async Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken ct = default)
	{
		// Build query string
		var query = $"auth/getapplications?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		// Make HTTP request
		var response = await _httpClient.GetFromJsonAsync<ApplicationsResponseDTO>(query, ct);

		// Handle null response
		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the applications Successfully");

			// Return an empty PaginatedResult
			return new PaginatedResult<ApplicationsDTO>(
				pageIndex: PageNumber ?? 1,
				pageSize: PageSize ?? 10,
				count: 0,
				data: Enumerable.Empty<ApplicationsDTO>()
			);
		}

		return response.applications!;
	}

	public async Task<PaginatedResult<SubMenusDTO>> GetSubMenusAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken ct = default)
	{
		// Build query string
		var query = $"auth/getsubmenus?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		// Make HTTP request
		var response = await _httpClient.GetFromJsonAsync<SubMenusResponseDTO>(query, ct);

		// Handle null response
		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the submenus Successfully");

			// Return an empty PaginatedResult
			return new PaginatedResult<SubMenusDTO>(
				pageIndex: PageNumber ?? 1,
				pageSize: PageSize ?? 10,
				count: 0,
				data: Enumerable.Empty<SubMenusDTO>()
			);
		}

		return response.submenus!;
	}

	public async Task<PaginatedResult<RolesDTO>> GetRolesAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken ct = default)
	{
		// Build query string
		var query = $"auth/getroles?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		// Make HTTP request
		var response = await _httpClient.GetFromJsonAsync<RolesResponseDTO>(query, ct);

		// Handle null response
		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the roles Successfully");

			// Return an empty PaginatedResult
			return new PaginatedResult<RolesDTO>(
				pageIndex: PageNumber ?? 1,
				pageSize: PageSize ?? 10,
				count: 0,
				data: Enumerable.Empty<RolesDTO>()
			);
		}

		return response.roles!;
	}

	public async Task<PaginatedResult<AppSubRolesDTO>> GetAppSubRolesAsync(int? PageNumber = 1, int? PageSize = 10, string? SearchTerm = null, CancellationToken ct = default)
	{
		// Build query string
		var query = $"auth/getappsubroles?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		// Make HTTP request
		var response = await _httpClient.GetFromJsonAsync<AppSubRolesResponseDTO>(query, ct);

		// Handle null response
		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the appsubroles Successfully");

			// Return an empty PaginatedResult
			return new PaginatedResult<AppSubRolesDTO>(
				pageIndex: PageNumber ?? 1,
				pageSize: PageSize ?? 10,
				count: 0,
				data: Enumerable.Empty<AppSubRolesDTO>()
			);
		}

		return response.appsubroles!;
	}

	public async Task<bool> DeleteApplicationAsync(int AppId)
	{
		var response = await _httpClient.DeleteAsync($"auth/deleteapplication/{AppId}");
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Delete the Application Successfully");
			return false!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Deleted the Application Successfully");
		return successContent!;
	}

	public async Task<bool> DeleteSubMenuAsync(int SubMenuId)
	{
		var response = await _httpClient.DeleteAsync($"auth/deletesubmenu/{SubMenuId}");
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Delete the SubMenu Successfully");
			return false!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Deleted the SubMenu Successfully");
		return successContent!;
	}

	public async Task<bool> DeleteRoleAsync(int RoleId)
	{
		var response = await _httpClient.DeleteAsync($"auth/deleterole/{RoleId}");
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Delete the Role Successfully");
			return false!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Deleted the Role Successfully");
		return successContent!;
	}

	public async Task<bool> DeleteUserAppSbRoleAsync(int AppSubRoleId)
	{
		var response = await _httpClient.DeleteAsync($"auth/deleteappsubrole/{AppSubRoleId}");
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Delete the AppSubRole Successfully");
			return false!;
		}

		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Deleted the AppSubRole Successfully");
		return successContent!;
	}

	public async Task<bool> AddApplicationAsync(AddApplicationDTO addApplicationDTO)
	{
		var response = await _httpClient.PostAsJsonAsync($"auth/addapplication", addApplicationDTO);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Add the Application Successfully");
			return false!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Added the Application Successfully");
		return successContent!;
	}
}
