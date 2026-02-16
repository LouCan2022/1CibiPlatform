namespace AIAgent.Skills.PolicyQuestionAnswering;

public sealed class PolicyQuestionAnswering : ISkill
{
	private readonly IPolicyRepository _policyRepository;
	private readonly IExcelQuestionExtractor _excelExtractor;
	private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
	private readonly IFileStorageService _fileStorageService;
	private readonly Kernel _kernel;
	private readonly ILogger<PolicyQuestionAnswering> _logger;

	public PolicyQuestionAnswering(
		IPolicyRepository policyRepository,
		IExcelQuestionExtractor excelExtractor,
		IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
		IFileStorageService fileStorageService,
		Kernel kernel,
		ILogger<PolicyQuestionAnswering> logger)
	{
		_policyRepository = policyRepository;
		_excelExtractor = excelExtractor;
		_embeddingGenerator = embeddingGenerator;
		_fileStorageService = fileStorageService;
		_kernel = kernel;
		_logger = logger;
	}

	public async Task<object?> RunAsync(
		JsonElement payload,
		CancellationToken cancellationToken = default)
	{
		if (!payload.TryGetProperty("Base64File", out var fileDataElement) || fileDataElement.ValueKind != JsonValueKind.String)
		{
			throw new InvalidOperationException("Missing or invalid 'Base64File' property in payload.");
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

		_logger.LogInformation("Processing Q&A Excel file: {FileName}", fileName);

		var fileBytes = Convert.FromBase64String(base64Data);

		var questions = await _excelExtractor.ExtractQuestionsFromExcelAsync(fileBytes, cancellationToken);
		_logger.LogInformation("Extracted {Count} questions from Excel", questions.Count);

		var answeredQuestions = new List<QuestionAnswerDto>();
		int answeredCount = 0;

		foreach (var question in questions)
		{
			var answer = await FindAnswerFromVectorDatabase(question.Question, cancellationToken);
			answeredQuestions.Add(new QuestionAnswerDto(question.Question, answer));

			if (!string.IsNullOrEmpty(answer))
			{
				answeredCount++;
			}

			_logger.LogInformation("Answered question: {Question}", question.Question);

		}

		var answeredExcelBytes = await _excelExtractor.WriteAnswersToExcelAsync(answeredQuestions, cancellationToken);

		var outputFileName = $"Answered_{System.IO.Path.GetFileNameWithoutExtension(fileName)}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
		var savedFileName = await _fileStorageService.SaveFileAsync(answeredExcelBytes, outputFileName, cancellationToken);
		var downloadUrl = _fileStorageService.GetFileUrl(savedFileName);

		_logger.LogInformation("Successfully answered {AnsweredCount}/{TotalCount} questions and saved to {FileName}",
			answeredCount, questions.Count, savedFileName);

		return new
		{
			Message = $"Successfully answered {answeredCount} out of {questions.Count} questions from {fileName}. Download your answered file here",
			DownloadUrl = downloadUrl
		};
	}

	private async Task<string> FindAnswerFromVectorDatabase(
		string question,
		CancellationToken cancellationToken)
	{
		var questionEmbedding = await GenerateEmbedding(question, cancellationToken);

		var similarPolicies = await _policyRepository.SearchSimilarPoliciesAsync(questionEmbedding, topK: 5, cancellationToken);

		if (!similarPolicies.Any())
		{
			return "No relevant policy information found.";
		}

		var combinedContext = string.Join("\n\n", similarPolicies.Select(p =>
			$"Policy: {p.PolicyCode} - Section: {p.SectionCode}\n{p.Content}"));

		var answer = await GenerateAnswerWithLLM(question, combinedContext, cancellationToken);

		return answer;
	}

	private async Task<string> GenerateAnswerWithLLM(
		string question,
		string context,
		CancellationToken cancellationToken)
	{
		var template = """
			You are a policy expert assistant. Answer the following question based ONLY on the provided policy context.
			If the answer cannot be found in the context, say "I cannot find the answer in the available policies."
			Be concise and precise in your answer.

			Policy Context:
			{{$context}}

			Question: {{$question}}

			Answer:
			""";

		var args = new KernelArguments
		{
			["context"] = context,
			["question"] = question
		};

		var result = await _kernel.InvokePromptAsync(template, args);
		return result?.ToString().Trim() ?? "Unable to generate an answer.";
	}

	private async Task<Vector> GenerateEmbedding(
		string text,
		CancellationToken cancellationToken)
	{
		var results = await _embeddingGenerator.GenerateAsync(
			new[] { text },
			cancellationToken: cancellationToken
		);

		var embedding = results.FirstOrDefault();
		if (embedding == null)
		{
			throw new InvalidOperationException("No embedding was generated for the provided text.");
		}

		var embeddingArray = ((Embedding<float>)embedding).Vector.ToArray();
		return new Vector(embeddingArray);
	}
}

