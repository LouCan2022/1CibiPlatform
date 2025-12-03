namespace AIAgent.Data.Context;

public class AIAgentApplicationDBContext : DbContext
{
	public AIAgentApplicationDBContext(DbContextOptions<AIAgentApplicationDBContext> options) : base(options)
	{
	}

	public DbSet<PolicyEntity> Policies { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AIAgentApplicationDBContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
