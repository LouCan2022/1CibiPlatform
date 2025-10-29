namespace FrontendWebassembly.Services.Auth.Implementation;

public class AccessService : IAccessService
{
	private readonly LocalStorageService _localStorage;
	private const string _appIdKey = "AppId";
	private const string _subMenuKey = "SubMenuId";

	public AccessService(LocalStorageService localStorage)
	{
		_localStorage = localStorage;
	}


	public async Task<bool> HasAccessAsync(int appId, int subMenuId)
	{
		var apps = JsonSerializer.Deserialize<List<int>>(await _localStorage.GetItemAsync<string>(_appIdKey));
		var subMenus = JsonSerializer.Deserialize<List<List<int>>>(await _localStorage.GetItemAsync<string>(_subMenuKey));

		Console.WriteLine($"Apps: {string.Join(", ", apps ?? new List<int>())}");
		Console.WriteLine($"SubMenus: {string.Join(", ", subMenus?.SelectMany(sm => sm) ?? new List<int>())}");

		if (apps is null && subMenus is null)
		{
			return false;
		}

		if (!apps.Contains(appId))
		{
			Console.WriteLine($"AppId {appId} not found in user's apps.");
			return false;
		}

		var index = apps.IndexOf(appId);
		Console.WriteLine($"Index of AppId {appId}: {index}");

		if (!subMenus[index].Contains(subMenuId))
		{
			Console.WriteLine($"SubMenuId {subMenuId} not found for AppId {appId}.");
			return false;
		}


		return true;
	}

}
