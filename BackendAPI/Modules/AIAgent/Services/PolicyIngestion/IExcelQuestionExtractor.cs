namespace AIAgent.Services.PolicyIngestion;

public interface IExcelQuestionExtractor
{
	Task<List<QuestionAnswerDto>> ExtractQuestionsFromExcelAsync(
		byte[] fileBytes,
		CancellationToken cancellationToken = default);

	Task<byte[]> WriteAnswersToExcelAsync(
		List<QuestionAnswerDto> questionsWithAnswers,
		CancellationToken cancellationToken = default);
}
