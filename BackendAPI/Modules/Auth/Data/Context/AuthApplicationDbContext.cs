
namespace Auth.Data.Context;
public class AuthApplicationDbContext : DbContext
{
	public AuthApplicationDbContext(DbContextOptions<AuthApplicationDbContext> options) : base(options)
	{
	}

	public DbSet<Authusers> AuthUsers { get; set; }

	public DbSet<AuthApplication> AuthApplications { get; set; }

	public DbSet<AuthRole> AuthRoles { get; set; }

	public DbSet<AuthUserAppRole> AuthUserAppRoles { get; set; }

	public DbSet<AuthRefreshToken> AuthRefreshToken { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthApplicationDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}

}
