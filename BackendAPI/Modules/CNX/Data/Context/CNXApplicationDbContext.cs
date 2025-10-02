namespace CNX.Data.Context;

public class CNXApplicationDbContext : DbContext
{
    public CNXApplicationDbContext(DbContextOptions<CNXApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<CNXTalkpush> CNXTalkpushes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CNXApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
