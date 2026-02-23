namespace AIAgent.Services.PolicyIngestion;

public class ExcelPolicyExtractorService : IExcelPolicyExtractorService
{
	private readonly ILogger<ExcelPolicyExtractorService> _logger;

	public ExcelPolicyExtractorService(ILogger<ExcelPolicyExtractorService> logger)
	{
		_logger = logger;
	}

	public async Task<List<PolicyDataDto>> ExtractPoliciesFromExcelAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
	{
		var policies = new List<PolicyDataDto>();

		using var stream = new MemoryStream(fileBytes);
		using var workbook = new XLWorkbook(stream);
		var worksheet = workbook.Worksheet(1); // First sheet

		// Skip header row, start from row 2
		var rows = worksheet.RowsUsed().Skip(1);

		foreach (var row in rows)
		{
			var policyName = row.Cell(1).GetValue<string>(); // Column A: Policy Name
			var sectionName = row.Cell(2).GetValue<string>(); // Column B: Section Name
			var content = row.Cell(3).GetValue<string>(); // Column C: Content

			if (string.IsNullOrWhiteSpace(policyName) || string.IsNullOrWhiteSpace(sectionName) || string.IsNullOrWhiteSpace(content))
			{
				_logger.LogWarning("Skipping row with empty data at row {RowNumber}", row.RowNumber());
				continue;
			}

			policies.Add(new PolicyDataDto(
				policyName.Trim(),
				sectionName.Trim(),
				content.Trim()
			));
		}

		return await Task.FromResult(policies);
	}
}
