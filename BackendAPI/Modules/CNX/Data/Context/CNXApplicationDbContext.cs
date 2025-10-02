namespace CNX.Data.Context;

public class CNXApplicationDbContext : DbContext
{
    public CNXApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
}
