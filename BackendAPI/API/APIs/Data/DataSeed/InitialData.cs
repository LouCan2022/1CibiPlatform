namespace Auth.Data.DataSeed.DataExtensions;

internal class InitialData
{
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly Guid _Id;

    public InitialData(IPasswordHasherService passwordHasherService)
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
                    FirstName = "admin",
                    LastName = "admin",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
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
                    RoleId = 1,
                    AssignedBy = this._Id,
                    AssignedAt = DateTime.UtcNow
                },
                new AuthUserAppRole
                {
                    UserId = this._Id,
                    AppId = 2,
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
                    RoleId = 1,
                    RoleName = "Admin",
                    Description = "Administrator Role",
                    CreatedAt = DateTime.UtcNow
                },
                new AuthRole
                {
                    RoleId = 2,
                    RoleName = "User",
                    Description = "User Role",
                    CreatedAt = DateTime.UtcNow
                }
            };
    }



}
