namespace AIAgent.Services.PolicyIngestion;

public interface IExcelPolicyExtractorService
{
	Task<List<PolicyDataDto>> ExtractPoliciesFromExcelAsync(byte[] fileBytes, CancellationToken cancellationToken = default);
}
