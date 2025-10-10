namespace FrontendWebassembly.Services.Auth.Interfaces;

public interface IConstructMenu
{
	Task<StringBuilder> ConstructMenu(
		List<int> appId,
		List<List<int>> menuId,
		List<int> roleId);
}
