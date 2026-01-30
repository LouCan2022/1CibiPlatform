using Microsoft.Extensions.AI;


namespace AIAgent.Skills.PolicyIngestion;

public sealed class PolicyIngestion : ISkill
{
	private readonly AIAgentApplicationDBContext _dbContext;
	private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
	private readonly ILogger<PolicyIngestion> _logger;
	private const int ChunkSize = 1000; // Characters per chunk
	private const int ChunkOverlap = 200; // Overlap between chunks

	public PolicyIngestion(
		AIAgentApplicationDBContext dbContext,
		IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
		ILogger<PolicyIngestion> logger)
	{
		_dbContext = dbContext;
		_embeddingGenerator = embeddingGenerator;
		_logger = logger;
	}

	public async Task<object?> RunAsync(
		JsonElement payload,
		CancellationToken cancellationToken = default)
	{
		// Extract file data from payload
		if (!payload.TryGetProperty("Base64File", out var fileDataElement) || fileDataElement.ValueKind != JsonValueKind.String)
		{
			throw new InvalidOperationException("Missing or invalid 'fileData' property in payload.");
		}

		if (!payload.TryGetProperty("FileName", out var fileNameElement) || fileNameElement.ValueKind != JsonValueKind.String)
		{
			throw new InvalidOperationException("Missing or invalid 'FileName' property in payload.");
		}

		var base64Data = fileDataElement.GetString();
		var fileName = fileNameElement.GetString();

		if (string.IsNullOrEmpty(base64Data) || string.IsNullOrEmpty(fileName))
		{
			throw new InvalidOperationException("File data or file name is empty.");
		}

		_logger.LogInformation("Processing Excel file: {FileName}", fileName);

		// Convert base64 to bytes
		var fileBytes = Convert.FromBase64String(base64Data);

		// Process Excel file
		var policies = await ExtractPoliciesFromExcel(fileBytes, cancellationToken);
		_logger.LogInformation("Extracted {Count} policy entries from Excel", policies.Count);

		// Collect all policy entities in a list
		var policyEntities = new List<AIPolicyEntity>();
		var totalChunks = 0;

		foreach (var policy in policies)
		{
			var chunks = ChunkPolicy(policy.Content);
			_logger.LogInformation("Chunked policy {PolicyCode}-{SectionCode} into {ChunkCount} chunks",
				policy.PolicyCode, policy.SectionCode, chunks.Count);

			// Generate embeddings for each chunk
			for (int i = 0; i < chunks.Count; i++)
			{
				var chunk = chunks[i];
				var embedding = await GenerateEmbedding(chunk, cancellationToken);

				var policyEntity = new AIPolicyEntity
				{
					PolicyCode = policy.PolicyCode,
					SectionCode = policy.SectionCode,
					DocumentName = fileName,
					Content = chunk,
					ChunckId = i + 1,
					Embedding = embedding
				};

				policyEntities.Add(policyEntity);
				totalChunks++;
			}
		}

		// Add all entities in one operation
		_dbContext.AIPolicies.AddRange(policyEntities);
		await _dbContext.SaveChangesAsync(cancellationToken);

		_logger.LogInformation("Successfully saved {TotalChunks} chunks with embeddings to database", totalChunks);

		return new
		{
			Success = true,
			Message = $"Successfully ingested {policies.Count} policies ({totalChunks} chunks) from {fileName}",
			PoliciesProcessed = policies.Count,
			TotalChunks = totalChunks
		};
	}

	private async Task<List<PolicyData>> ExtractPoliciesFromExcel(byte[] fileBytes, CancellationToken cancellationToken)
	{
		var policies = new List<PolicyData>();

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

			policies.Add(new PolicyData
			{
				PolicyCode = policyName.Trim(),
				SectionCode = sectionName.Trim(),
				Content = content.Trim()
			});
		}

		return policies;
	}

	private List<string> ChunkPolicy(string content)
	{
		var chunks = new List<string>();

		if (string.IsNullOrWhiteSpace(content))
		{
			return chunks;
		}

		var contentLength = content.Length;
		var startIndex = 0;

		while (startIndex < contentLength)
		{
			var length = Math.Min(ChunkSize, contentLength - startIndex);
			var chunk = content.Substring(startIndex, length);

			// Try to break at sentence boundary if possible
			if (startIndex + length < contentLength)
			{
				var lastPeriod = chunk.LastIndexOf('.');
				var lastNewline = chunk.LastIndexOf('\n');
				var breakPoint = Math.Max(lastPeriod, lastNewline);

				if (breakPoint > ChunkSize / 2) // Only break if we're past halfway
				{
					chunk = content.Substring(startIndex, breakPoint + 1);
					length = breakPoint + 1;
				}
			}

			chunks.Add(chunk.Trim());
			startIndex += length - ChunkOverlap;

			// Ensure we don't go backwards
			if (startIndex < 0 || startIndex >= contentLength)
			{
				break;
			}
		}

		return chunks;
	}

	private async Task<Vector> GenerateEmbedding(string text, CancellationToken cancellationToken)
	{
		// Use the correct method from IEmbeddingGenerator interface
		var results = await _embeddingGenerator.GenerateAsync(
			new[] { text },
			cancellationToken: cancellationToken
		);

		// Get the first embedding from the result (GeneratedEmbeddings<TEmbedding> implements IList<TEmbedding>)
		var embedding = results.FirstOrDefault();
		if (embedding == null)
		{
			throw new InvalidOperationException("No embedding was generated for the provided text.");
		}

		// Convert ReadOnlyMemory<float> to float[]
		var embeddingArray = ((Embedding<float>)embedding).Vector.ToArray();

		return new Vector(embeddingArray);
	}

	private class PolicyData
	{
		public string PolicyCode { get; set; } = string.Empty;
		public string SectionCode { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
	}
}
