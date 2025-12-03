namespace FrontendWebassembly.ShareData.Auth;

public static class SubMenuList
{
	public static Dictionary<int, (string path, string Name, string Icon)> List =>
	  new()
	  {
		{ 1, ("cnxdashboard", "List of Subjects" , Icons.Material.Filled.Dashboard) },
		{ 2, ("idv", "IDV" , Icons.Material.Filled.Person) },
		{ 3, ("usermanagement", "User Management" , Icons.Material.Filled.ManageAccounts) }
	  };
}

