namespace FrontendWebassembly.ShareData.Auth;

public static class SubMenuList
{
	public static Dictionary<int, (string path, string Name, string Icon)> List =>
	  new()
	  {
		{ 1, ("dashboard", "Dashboard" , Icons.Material.Filled.Dashboard) },
		{ 2, ("idv", "IDV" , Icons.Material.Filled.Person) },
		{ 3, ("usersearch", "User Search" , Icons.Material.Filled.Search) }
	  };
}
