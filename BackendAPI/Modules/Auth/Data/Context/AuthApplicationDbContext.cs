namespace Auth.Data.Context;
public class AuthApplicationDbContext : DbContext
{
    public AuthApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserAppRole> UserAppRoles => Set<UserAppRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthApplicationDbContext).Assembly);
    }

}
