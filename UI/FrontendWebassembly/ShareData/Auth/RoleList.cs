namespace FrontendWebassembly.ShareData.Auth;

public static class RoleList
{
	public static Dictionary<int, string> List =>
	  new()
	  {
		{ 1, "SuperAdmin" },
		{ 2, "Admin" },
		{ 3, "User" }
	  };
}
