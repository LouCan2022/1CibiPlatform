namespace AIAgent.Data.Repositories;

public class PolicyRepository : IPolicyRepository
{
	private readonly AIAgentApplicationDBContext _dbContext;

	public PolicyRepository(AIAgentApplicationDBContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task AddPoliciesAsync(IEnumerable<AIPolicyEntity> policies, CancellationToken cancellationToken = default)
	{
		_dbContext.AIPolicies.AddRange(policies);
		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	public async Task<List<AIPolicyEntity>> SearchSimilarPoliciesAsync(
		Vector queryEmbedding,
		int topK = 5,
		CancellationToken cancellationToken = default)
	{
		var results = await _dbContext.AIPolicies
					.FromSqlRaw(@"
                        SELECT * FROM ai.""AIPolicy""
                        ORDER BY ""Embedding"" <-> {0}
                        LIMIT {1}
                    ", queryEmbedding, topK)
					.ToListAsync(cancellationToken);

		return results;
	}
}
