namespace AIAgent.Data.Repositories;

public interface IPolicyRepository
{
	Task AddPoliciesAsync(IEnumerable<AIPolicyEntity> policies, CancellationToken cancellationToken = default);
	Task<List<AIPolicyEntity>> SearchSimilarPoliciesAsync(Vector queryEmbedding, int topK = 5, CancellationToken cancellationToken = default);
}
