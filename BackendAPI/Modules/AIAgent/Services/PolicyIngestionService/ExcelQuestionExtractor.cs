namespace AIAgent.Services.PolicyIngestion;

public class ExcelQuestionExtractor : IExcelQuestionExtractor
{
	private readonly ILogger<ExcelQuestionExtractor> _logger;

	public ExcelQuestionExtractor(ILogger<ExcelQuestionExtractor> logger)
	{
		_logger = logger;
	}

	public async Task<List<QuestionAnswerDto>> ExtractQuestionsFromExcelAsync(byte[] fileBytes, CancellationToken cancellationToken = default)
	{
		var questions = new List<QuestionAnswerDto>();

		using var stream = new MemoryStream(fileBytes);
		using var workbook = new XLWorkbook(stream);
		var worksheet = workbook.Worksheet(1);

		var rows = worksheet.RowsUsed().Skip(1);

		foreach (var row in rows)
		{
			var question = row.Cell(1).GetValue<string>();
			var answer = row.Cell(2).GetValue<string>();

			if (string.IsNullOrWhiteSpace(question))
			{
				_logger.LogWarning("Skipping row with empty question at row {RowNumber}", row.RowNumber());
				continue;
			}

			questions.Add(new QuestionAnswerDto(question.Trim(), string.IsNullOrWhiteSpace(answer) ? null : answer.Trim()));
		}

		return await Task.FromResult(questions);
	}

	public async Task<byte[]> WriteAnswersToExcelAsync(
		List<QuestionAnswerDto> questionsWithAnswers,
		CancellationToken cancellationToken = default)
	{
		using var workbook = new XLWorkbook();
		var worksheet = workbook.Worksheets.Add("Q&A");

		worksheet.Cell(1, 1).Value = "Question";
		worksheet.Cell(1, 2).Value = "Answer";

		worksheet.Range(1, 1, 1, 2).Style.Font.Bold = true;
		worksheet.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightBlue;

		int rowIndex = 2;
		foreach (var qa in questionsWithAnswers)
		{
			worksheet.Cell(rowIndex, 1).Value = qa.Question;
			worksheet.Cell(rowIndex, 2).Value = qa.Answer ?? "No answer found";
			rowIndex++;
		}

		worksheet.Columns().AdjustToContents();

		using var stream = new MemoryStream();
		workbook.SaveAs(stream);
		return await Task.FromResult(stream.ToArray());
	}
}
