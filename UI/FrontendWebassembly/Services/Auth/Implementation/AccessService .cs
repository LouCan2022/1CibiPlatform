namespace FrontendWebassembly.Services.Auth.Implementation;

using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class AccessService : IAccessService
{
	private readonly LocalStorageService _localStorage;
	private readonly ILogger<AccessService> _logger;
	private const string _appIdKey = "AppId";
	private const string _subMenuKey = "SubMenuId";

	public AccessService(LocalStorageService localStorage, ILogger<AccessService> logger)
	{
		_localStorage = localStorage;
		_logger = logger;
	}


	public async Task<bool> HasAccessAsync(int appId, int subMenuId)
	{
		var apps = JsonSerializer.Deserialize<List<int>>(await _localStorage.GetItemAsync<string>(_appIdKey));
		var subMenus = JsonSerializer.Deserialize<List<List<int>>>(await _localStorage.GetItemAsync<string>(_subMenuKey));

		_logger.LogDebug("Apps: {Apps}", string.Join(", ", apps ?? new List<int>()));
		_logger.LogDebug("SubMenus: {SubMenus}", string.Join(", ", subMenus?.SelectMany(sm => sm) ?? new List<int>()));

		if (apps is null && subMenus is null)
		{
			return false;
		}

		if (!apps.Contains(appId))
		{
			_logger.LogWarning("AppId {AppId} not found in user's apps.", appId);
			return false;
		}

		var index = apps.IndexOf(appId);
		_logger.LogDebug("Index of AppId {AppId}: {Index}", appId, index);

		if (!subMenus[index].Contains(subMenuId))
		{
			_logger.LogWarning("SubMenuId {SubMenuId} not found for AppId {AppId}.", subMenuId, appId);
			return false;
		}


		return true;
	}

}
