using AIAgent.Data.Entities;
using AIAgent.DTO;
using FluentAssertions;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Pgvector;
using System.Text.Json;
using Test.BackendAPI.Modules.AIAgent.UnitTests.Fixture;

namespace Test.BackendAPI.Modules.AIAgent.UnitTests;

public class PolicyQuestionAnsweringTests : IClassFixture<PolicyQuestionAnsweringFixture>
{
	private readonly PolicyQuestionAnsweringFixture _fixture;

	public PolicyQuestionAnsweringTests(PolicyQuestionAnsweringFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task RunAsync_ShouldThrowException_WhenBase64FilePropertyIsMissing()
	{
		// Arrange
		var payload = JsonDocument.Parse("""
			{
				"FileName": "test.xlsx"
			}
			""").RootElement;

		// Act
		Func<Task> act = async () => await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Missing or invalid 'Base64File' property in payload.");
	}

	[Fact]
	public async Task RunAsync_ShouldThrowException_WhenFileNamePropertyIsMissing()
	{
		// Arrange
		var payload = JsonDocument.Parse("""
			{
				"Base64File": "dGVzdA=="
			}
			""").RootElement;

		// Act
		Func<Task> act = async () => await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("Missing or invalid 'FileName' property in payload.");
	}

	[Fact]
	public async Task RunAsync_ShouldThrowException_WhenBase64FileIsEmpty()
	{
		// Arrange
		var payload = JsonDocument.Parse("""
			{
				"Base64File": "",
				"FileName": "test.xlsx"
			}
			""").RootElement;

		// Act
		Func<Task> act = async () => await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("File data or file name is empty.");
	}

	[Fact]
	public async Task RunAsync_ShouldThrowException_WhenFileNameIsEmpty()
	{
		// Arrange
		var payload = JsonDocument.Parse("""
			{
				"Base64File": "dGVzdA==",
				"FileName": ""
			}
			""").RootElement;

		// Act
		Func<Task> act = async () => await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("File data or file name is empty.");
	}

	[Fact]
	public async Task RunAsync_ShouldProcessQuestionsAndReturnResult_WhenValidPayload()
	{
		// Arrange
		var base64File = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
		var fileName = "questions.xlsx";
		var payload = JsonDocument.Parse($$$"""
			{
				"Base64File": "{{{base64File}}}",
				"FileName": "{{{fileName}}}"
			}
			""").RootElement;

		var extractedQuestions = new List<QuestionAnswerDto>
		{
			new QuestionAnswerDto("What is policy 101?"),
			new QuestionAnswerDto("What is policy 102?")
		};

		var similarPolicies = new List<AIPolicyEntity>
		{
			new AIPolicyEntity
			{
				Id = 1,
				PolicyCode = "POL-101",
				SectionCode = "SEC-1",
				Content = "This is policy 101 content",
				DocumentName = "Policy Document 1",
				ChunckId = 1,
				Embedding = new Vector(new float[] { 0.1f, 0.2f, 0.3f })
			}
		};

		var answeredExcelBytes = new byte[] { 10, 20, 30 };
		var savedFileName = "Answered_questions_20260101120000.xlsx";
		var downloadUrl = "https://storage.example.com/files/" + savedFileName;

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.ExtractQuestionsFromExcelAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(extractedQuestions);

		// Setup embedding generator
		var mockEmbedding = new Embedding<float>(new float[] { 0.1f, 0.2f, 0.3f });
		_fixture.MockEmbeddingGenerator
			.Setup(x => x.GenerateAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult<GeneratedEmbeddings<Embedding<float>>>(new GeneratedEmbeddings<Embedding<float>>([mockEmbedding])));

		_fixture.MockPolicyRepository
			.Setup(x => x.SearchSimilarPoliciesAsync(It.IsAny<Vector>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(similarPolicies);

		// Setup chat completion service to return a mock response
		var chatResponse = new ChatMessageContent(
			AuthorRole.Assistant,
			"Answer to the question");

		_fixture.MockChatCompletionService
			.Setup(x => x.GetChatMessageContentsAsync(
				It.IsAny<ChatHistory>(),
				It.IsAny<PromptExecutionSettings>(),
				It.IsAny<Kernel>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<ChatMessageContent> { chatResponse });

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.WriteAnswersToExcelAsync(It.IsAny<List<QuestionAnswerDto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(answeredExcelBytes);

		_fixture.MockFileStorageService
			.Setup(x => x.SaveFileAsync(answeredExcelBytes, It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(savedFileName);

		_fixture.MockFileStorageService
			.Setup(x => x.GetFileUrl(savedFileName))
			.Returns(downloadUrl);

		// Act
		var result = await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		var resultDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(result));
		resultDict.Should().ContainKey("Message");
		resultDict.Should().ContainKey("DownloadUrl");
		resultDict!["DownloadUrl"].ToString().Should().Be(downloadUrl);
	}

	[Fact]
	public async Task RunAsync_ShouldCallExcelExtractorWithCorrectBytes()
	{
		// Arrange
		var testBytes = new byte[] { 1, 2, 3, 4, 5 };
		var base64File = Convert.ToBase64String(testBytes);
		var fileName = "test.xlsx";
		var payload = JsonDocument.Parse($$$"""
			{
				"Base64File": "{{{base64File}}}",
				"FileName": "{{{fileName}}}"
			}
			""").RootElement;

		var extractedQuestions = new List<QuestionAnswerDto>
		{
			new QuestionAnswerDto("Test question?")
		};

		byte[]? capturedBytes = null;
		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.ExtractQuestionsFromExcelAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.Callback<byte[], CancellationToken>((bytes, ct) => capturedBytes = bytes)
			.ReturnsAsync(extractedQuestions);

		var mockEmbedding = new Embedding<float>(new float[] { 0.1f, 0.2f });
		_fixture.MockEmbeddingGenerator
			.Setup(x => x.GenerateAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult<GeneratedEmbeddings<Embedding<float>>>(new GeneratedEmbeddings<Embedding<float>>([mockEmbedding])));

		_fixture.MockPolicyRepository
			.Setup(x => x.SearchSimilarPoliciesAsync(It.IsAny<Vector>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AIPolicyEntity>());

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.WriteAnswersToExcelAsync(It.IsAny<List<QuestionAnswerDto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new byte[] { 10, 20 });

		_fixture.MockFileStorageService
			.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync("file.xlsx");

		_fixture.MockFileStorageService
			.Setup(x => x.GetFileUrl(It.IsAny<string>()))
			.Returns("https://example.com/file.xlsx");

		// Act
		await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		capturedBytes.Should().NotBeNull();
		capturedBytes.Should().BeEquivalentTo(testBytes);
	}

	[Fact]
	public async Task RunAsync_ShouldCallEmbeddingGeneratorForEachQuestion()
	{
		// Arrange
		var base64File = Convert.ToBase64String(new byte[] { 1, 2, 3 });
		var fileName = "test.xlsx";
		var payload = JsonDocument.Parse($$$"""
			{
				"Base64File": "{{{base64File}}}",
				"FileName": "{{{fileName}}}"
			}
			""").RootElement;

		var extractedQuestions = new List<QuestionAnswerDto>
		{
			new QuestionAnswerDto("Question 1?"),
			new QuestionAnswerDto("Question 2?"),
			new QuestionAnswerDto("Question 3?")
		};

		var callCount = 0;
		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.ExtractQuestionsFromExcelAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(extractedQuestions);

		var mockEmbedding = new Embedding<float>(new float[] { 0.1f, 0.2f });
		_fixture.MockEmbeddingGenerator
			.Setup(x => x.GenerateAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
			.Callback(() => callCount++)
			.Returns(Task.FromResult<GeneratedEmbeddings<Embedding<float>>>(new GeneratedEmbeddings<Embedding<float>>([mockEmbedding])));

		_fixture.MockPolicyRepository
			.Setup(x => x.SearchSimilarPoliciesAsync(It.IsAny<Vector>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AIPolicyEntity>());

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.WriteAnswersToExcelAsync(It.IsAny<List<QuestionAnswerDto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new byte[] { 10, 20 });

		_fixture.MockFileStorageService
			.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync("file.xlsx");

		_fixture.MockFileStorageService
			.Setup(x => x.GetFileUrl(It.IsAny<string>()))
			.Returns("https://example.com/file.xlsx");

		// Act
		await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		callCount.Should().Be(3, "embedding generator should be called once per question");
	}

	[Fact]
	public async Task RunAsync_ShouldCallPolicyRepositorySearchForEachQuestion()
	{
		// Arrange
		var base64File = Convert.ToBase64String(new byte[] { 1, 2, 3 });
		var fileName = "test.xlsx";
		var payload = JsonDocument.Parse($$$"""
			{
				"Base64File": "{{{base64File}}}",
				"FileName": "{{{fileName}}}"
			}
			""").RootElement;

		var extractedQuestions = new List<QuestionAnswerDto>
		{
			new QuestionAnswerDto("Question 1?"),
			new QuestionAnswerDto("Question 2?")
		};

		var searchCallCount = 0;
		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.ExtractQuestionsFromExcelAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(extractedQuestions);

		var mockEmbedding = new Embedding<float>(new float[] { 0.1f, 0.2f });
		_fixture.MockEmbeddingGenerator
			.Setup(x => x.GenerateAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult<GeneratedEmbeddings<Embedding<float>>>(new GeneratedEmbeddings<Embedding<float>>([mockEmbedding])));

		_fixture.MockPolicyRepository
			.Setup(x => x.SearchSimilarPoliciesAsync(It.IsAny<Vector>(), 5, It.IsAny<CancellationToken>()))
			.Callback(() => searchCallCount++)
			.ReturnsAsync(new List<AIPolicyEntity>());

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.WriteAnswersToExcelAsync(It.IsAny<List<QuestionAnswerDto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new byte[] { 10, 20 });

		_fixture.MockFileStorageService
			.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync("file.xlsx");

		_fixture.MockFileStorageService
			.Setup(x => x.GetFileUrl(It.IsAny<string>()))
			.Returns("https://example.com/file.xlsx");

		// Act
		await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		searchCallCount.Should().Be(2, "policy repository search should be called once per question");
	}

	[Fact]
	public async Task RunAsync_ShouldSaveFileWithCorrectNamingPattern()
	{
		// Arrange
		var base64File = Convert.ToBase64String(new byte[] { 1, 2, 3 });
		var fileName = "TestQuestions.xlsx";
		var payload = JsonDocument.Parse($$$"""
			{
				"Base64File": "{{{base64File}}}",
				"FileName": "{{{fileName}}}"
			}
			""").RootElement;

		var extractedQuestions = new List<QuestionAnswerDto>
		{
			new QuestionAnswerDto("Question?")
		};

		string? capturedFileName = null;
		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.ExtractQuestionsFromExcelAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(extractedQuestions);

		var mockEmbedding = new Embedding<float>(new float[] { 0.1f });
		_fixture.MockEmbeddingGenerator
			.Setup(x => x.GenerateAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult<GeneratedEmbeddings<Embedding<float>>>(new GeneratedEmbeddings<Embedding<float>>([mockEmbedding])));

		_fixture.MockPolicyRepository
			.Setup(x => x.SearchSimilarPoliciesAsync(It.IsAny<Vector>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AIPolicyEntity>());

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.WriteAnswersToExcelAsync(It.IsAny<List<QuestionAnswerDto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new byte[] { 10, 20 });

		_fixture.MockFileStorageService
			.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.Callback<byte[], string, CancellationToken>((bytes, name, ct) => capturedFileName = name)
			.ReturnsAsync("saved_file.xlsx");

		_fixture.MockFileStorageService
			.Setup(x => x.GetFileUrl(It.IsAny<string>()))
			.Returns("https://example.com/file.xlsx");

		// Act
		await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		capturedFileName.Should().NotBeNull();
		capturedFileName.Should().StartWith("Answered_TestQuestions_");
		capturedFileName.Should().EndWith(".xlsx");
	}

	[Fact]
	public async Task RunAsync_ShouldReturnCorrectDownloadUrl()
	{
		// Arrange
		var base64File = Convert.ToBase64String(new byte[] { 1, 2, 3 });
		var fileName = "test.xlsx";
		var payload = JsonDocument.Parse($$$"""
			{
				"Base64File": "{{{base64File}}}",
				"FileName": "{{{fileName}}}"
			}
			""").RootElement;

		var extractedQuestions = new List<QuestionAnswerDto>
		{
			new QuestionAnswerDto("Question?")
		};

		var expectedFileName = "saved_file.xlsx";
		var expectedUrl = "https://storage.example.com/files/saved_file.xlsx";

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.ExtractQuestionsFromExcelAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(extractedQuestions);

		var mockEmbedding = new Embedding<float>(new float[] { 0.1f });
		_fixture.MockEmbeddingGenerator
			.Setup(x => x.GenerateAsync(It.IsAny<IEnumerable<string>>(), null, It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult<GeneratedEmbeddings<Embedding<float>>>(new GeneratedEmbeddings<Embedding<float>>([mockEmbedding])));

		_fixture.MockPolicyRepository
			.Setup(x => x.SearchSimilarPoliciesAsync(It.IsAny<Vector>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<AIPolicyEntity>());

		_fixture.MockExcelQuestionExtractor
			.Setup(x => x.WriteAnswersToExcelAsync(It.IsAny<List<QuestionAnswerDto>>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new byte[] { 10, 20 });

		_fixture.MockFileStorageService
			.Setup(x => x.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedFileName);

		_fixture.MockFileStorageService
			.Setup(x => x.GetFileUrl(expectedFileName))
			.Returns(expectedUrl);

		// Act
		var result = await _fixture.PolicyQuestionAnsweringSkill.RunAsync(payload, CancellationToken.None);

		// Assert
		result.Should().NotBeNull();
		var resultDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(result));
		resultDict!["DownloadUrl"].ToString().Should().Be(expectedUrl);
	}
}
