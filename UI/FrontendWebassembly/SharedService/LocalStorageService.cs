namespace FrontendWebassembly.SharedService;

public class LocalStorageService
{
	private readonly IJSRuntime _jsRuntime;

	public LocalStorageService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	// Add or update an item in local storage
	public async Task SetItemAsync<T>(string key, T value)
	{
		var json = JsonSerializer.Serialize(value);
		await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
	}

	// Get an item from local storage
	public async Task<T?> GetItemAsync<T>(string key)
	{
		var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

		if (string.IsNullOrEmpty(json))
			return default;

		return JsonSerializer.Deserialize<T>(json);
	}

	// Remove an item from local storage
	public async Task RemoveItemAsync(string key)
	{
		await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
	}

	// Clear all items from local storage
	public async Task ClearAsync()
	{
		await _jsRuntime.InvokeVoidAsync("localStorage.clear");
	}

	// Check if a key exists in local storage
	public async Task<bool> ContainsKeyAsync(string key)
	{
		var value = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
		return value != null;
	}

	// Get the number of items in local storage
	public async Task<int> LengthAsync()
	{
		return await _jsRuntime.InvokeAsync<int>("localStorage.length");
	}
}