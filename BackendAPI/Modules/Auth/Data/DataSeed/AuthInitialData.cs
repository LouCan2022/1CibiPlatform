namespace Auth.Data.DataSeed;

public class AuthInitialData
{

	private readonly IPasswordHasherService _passwordHasherService;
	private readonly Guid _Id;

	public AuthInitialData(IPasswordHasherService passwordHasherService)
	{
		this._passwordHasherService = passwordHasherService;
		this._Id = Guid.NewGuid();
	}
	public IEnumerable<Authusers> GetUsers()
	{
		return new List<Authusers>
			{
				new Authusers
				{
					Id = this._Id,
					Username = "admin",
					Email = "john@example.com",
					PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
					FirstName = "Admin",
					LastName = ""
				},
				new Authusers
				{
					Id = Guid.NewGuid(),
					Username = "cbblueadmin",
					Email = "cb@example.com",
					PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
					FirstName = "CB",
					LastName = "Admin"
				},
			};
	}

	public IEnumerable<AuthUserAppRole> GetUserAppRoles()
	{
		return new List<AuthUserAppRole>
			{
				new AuthUserAppRole
				{
					UserId = this._Id,
					AppId = 1,
					Submenu = 1,
					RoleId = 1,
					AssignedBy = this._Id,
					AssignedAt = DateTime.UtcNow
				},
				new AuthUserAppRole
				{
					UserId = this._Id,
					AppId = 2,
					Submenu= 2,
					RoleId = 2,
					AssignedBy = this._Id,
					AssignedAt = DateTime.UtcNow
				}
			};
	}

	public IEnumerable<AuthApplication> GetApplications()
	{
		return new List<AuthApplication>
			{
				new AuthApplication
				{
					AppName = "CNX",
					AppCode = "1",
					Description = "Concentrix API",
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				},
				new AuthApplication
				{
					AppName = "Philsys",
					AppCode = "2",
					Description = "IDV",
					IsActive = true,
					CreatedAt = DateTime.UtcNow
				}
			};
	}


	public IEnumerable<AuthRole> GetRoles()
	{
		return new List<AuthRole>
			{
				new AuthRole
				{
					RoleName = "SuperAdmin",
					Description = "Super Admin",
					CreatedAt = DateTime.UtcNow
				},
				new AuthRole
				{
					RoleName = "Admin",
					Description = "Administrator Role",
					CreatedAt = DateTime.UtcNow
				},
				new AuthRole
				{
					RoleName = "User",
					Description = "User Role",
					CreatedAt = DateTime.UtcNow
				}
			};
	}

	//create for sub menu
	public IEnumerable<AuthSubMenu> GetSubMenus()
	{
		return new List<AuthSubMenu>
			{
				new AuthSubMenu
				{
					SubMenuName = "Dashboard",
					Description = "Dashboard SubMenu",
					CreatedAt = DateTime.UtcNow
				},
				new AuthSubMenu
				{
					SubMenuName = "IDV",
					Description = "Philsys IDV",
					CreatedAt = DateTime.UtcNow
				}
			};
	}

}
