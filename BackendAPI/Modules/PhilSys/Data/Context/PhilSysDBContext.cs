namespace PhilSys.Data.Context;

public class PhilSysDBContext : DbContext
{
	public PhilSysDBContext(DbContextOptions<PhilSysDBContext> options) : base(options)
	{
		
	}
	public DbSet<PhilSysTransaction> PhilSysTransactions { get; set; }
	public DbSet<PhilSysTransactionResult> PhilSysTransactionResults { get; set; }
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(PhilSysDBContext).Assembly);

		modelBuilder.Entity<PhilSysTransactionResult>(builder =>
		{
			// Make DataSubject owned
			builder.OwnsOne(e => e.data_subject, ds =>
			{
				// DataSubject’s nested owned types
				ds.OwnsOne(d => d.address);
				ds.OwnsOne(d => d.place_of_birth);
			});
		});


		base.OnModelCreating(modelBuilder);
	}
}
