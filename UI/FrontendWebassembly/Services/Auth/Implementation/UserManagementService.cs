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
		var query = $"auth/getusers?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		var response = await _httpClient.GetFromJsonAsync<UsersResponseDTO>(query, ct);

		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the Users Successfully");

			return new PaginatedResult<UsersDTO>(
				pageIndex: PageNumber ?? 1,
				pageSize: PageSize ?? 10,
				count: 0,
				data: Enumerable.Empty<UsersDTO>()
			);
		}

		return response.users!;
	}

	public async Task<PaginatedResult<ApplicationsDTO>> GetApplicationsAsync(int? PageNumber = 1, int? PageSize = int.MaxValue, string? SearchTerm = null, CancellationToken ct = default)
	{
		var query = $"auth/getapplications?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		var response = await _httpClient.GetFromJsonAsync<ApplicationsResponseDTO>(query, ct);

		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the applications Successfully");

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
		var query = $"auth/getsubmenus?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		var response = await _httpClient.GetFromJsonAsync<SubMenusResponseDTO>(query, ct);

		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the submenus Successfully");

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
		var query = $"auth/getroles?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		var response = await _httpClient.GetFromJsonAsync<RolesResponseDTO>(query, ct);

		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the roles Successfully");

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
		var query = $"auth/getappsubroles?pageNumber={PageNumber}&pageSize={PageSize}";
		if (!string.IsNullOrEmpty(SearchTerm))
			query += $"&SearchTerm={Uri.EscapeDataString(SearchTerm)}";

		var response = await _httpClient.GetFromJsonAsync<AppSubRolesResponseDTO>(query, ct);

		if (response == null)
		{
			Console.WriteLine("❌ Did not Get the appsubroles Successfully");

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

	public async Task<bool> AddApplicationAsync(AddApplicationDTO application)
	{
		var payload = new
		{
			application
		};

		var response = await _httpClient.PostAsJsonAsync($"auth/addapplication", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Add the Application Successfully");
			return false!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Added the Application Successfully");
		return successContent;
	}

	public async Task<bool> AddSubMenuAsync(AddSubMenuDTO subMenu)
	{
		var payload = new
		{
			subMenu
		};

		var response = await _httpClient.PostAsJsonAsync($"auth/addsubmenu", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Add the SubMenu Successfully");
			return false!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Added the SubMenu Successfully");
		return successContent;
	}

	public async Task<bool> AddRoleAsync(AddRoleDTO role)
	{
		var payload = new
		{
			role
		};

		var response = await _httpClient.PostAsJsonAsync($"auth/addrole", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Add the Role Successfully");
			return false!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Added the Role Successfully");
		return successContent;
	}

	public async Task<bool> AddAppSubRoleAsync(AddAppSubRoleDTO appSubRole)
	{
		var payload = new
		{
			appSubRole
		};

		var response = await _httpClient.PostAsJsonAsync($"auth/addappsubrole", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Add the User's AppSubRole Successfully");
			return false!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Added the User's AppSubRole Successfully");
		return successContent;
	}

	public async Task<bool> SendNotificationAsync(AssignmentNotificationDTO accountNotificationDTO)
	{
		var payload = new { accountNotificationDTO };
		var response = await _httpClient.PostAsJsonAsync("account/notification", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not able to send the notification to user's email.");
			return false!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<bool>();
		Console.WriteLine("✅ Sent the notification successfully to user's email");
		return successContent;
	}

	public async Task<EditApplicationDTO> EditApplicationAsync(ApplicationsDTO editApplicationDTO)
	{
		var editApplication = new EditApplicationDTO
		{
			AppId = editApplicationDTO.applicationId,
			AppName = editApplicationDTO.applicationName,
			Description = editApplicationDTO.Description,
			IsActive = editApplicationDTO.IsActive
		};

		var payload = new
		{
			editApplication
		};

		var response = await _httpClient.PatchAsJsonAsync($"auth/editapplication", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Edit the Application Successfully");
			return null!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<EditApplicationDTO>();
		Console.WriteLine("✅ Added the Application Successfully");
		if (successContent != null)
		{
			return successContent;
		}
		return null!;
	}

	public async Task<EditSubMenuDTO> EditSubMenuAsync(SubMenusDTO editSubMenuDTO)
	{
		var editSubMenu = new EditSubMenuDTO
		{
			SubMenuId = editSubMenuDTO.subMenuId,
			SubMenuName = editSubMenuDTO.subMenuName,
			Description = editSubMenuDTO.Description,
			IsActive = editSubMenuDTO.IsActive
		};

		var payload = new
		{
			editSubMenu
		};

		var response = await _httpClient.PatchAsJsonAsync($"auth/editsubmenu", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Edit the SubMenu Successfully");
			return null!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<EditSubMenuDTO>();
		Console.WriteLine("✅ Added the SubMenu Successfully");
		if (successContent != null)
		{
			return successContent;
		}
		return null!;
	}

	public async Task<EditRoleDTO> EditRoleAsync(RolesDTO editRoleDTO)
	{
		var editRole = new EditRoleDTO
		{
			RoleId = editRoleDTO.roleId,
			RoleName = editRoleDTO.roleName,
			Description = editRoleDTO.Description
		};

		var payload = new
		{
			editRole
		};

		var response = await _httpClient.PatchAsJsonAsync($"auth/editrole", payload);
		if (!response.IsSuccessStatusCode)
		{
			Console.WriteLine("❌ Did not Edit the Role Successfully");
			return null!;
		}
		var successContent = await response.Content.ReadFromJsonAsync<EditRoleDTO>();
		Console.WriteLine("✅ Added the Role Successfully");
		if (successContent != null)
		{
			return successContent;
		}
		return null!;
	}
    public async Task<EditAppSubRoleDTO> EditAppSubRoleAsync(AppSubRolesDTO editAppSubRoleDTO)
    {
        var editAppSubRole = new EditAppSubRoleDTO
        {
            AppSubRoleId = editAppSubRoleDTO.AppRoleId,
            UserId = editAppSubRoleDTO.UserId,
            AppId = editAppSubRoleDTO.AppId,
            SubMenuId = editAppSubRoleDTO.SubMenuId,
            RoleId = editAppSubRoleDTO.RoleId,
        };

        var payload = new
        {
            editAppSubRole
        };

        var response = await _httpClient.PatchAsJsonAsync($"auth/editappsubrole", payload);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("❌ Did not Edit the UserAppSubRole Successfully");
            return null!;
        }
        var successContent = await response.Content.ReadFromJsonAsync<EditAppSubRoleDTO>();
        Console.WriteLine("✅ Added the UserAppSubRole Successfully");
        if (successContent != null)
        {
            return successContent;
        }
        return null!;
    }
}
