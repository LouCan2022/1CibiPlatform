using AIAgent.Data.Repositories;
using AIAgent.Services;
using AIAgent.Skills.PolicyQuestionAnswering;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;

namespace Test.BackendAPI.Modules.AIAgent.UnitTests.Fixture;

public class PolicyQuestionAnsweringFixture : IDisposable
{
	// Mocks
	public Mock<IPolicyRepository> MockPolicyRepository { get; private set; }
	public Mock<IExcelQuestionExtractor> MockExcelQuestionExtractor { get; private set; }
	public Mock<IEmbeddingGenerator<string, Embedding<float>>> MockEmbeddingGenerator { get; private set; }
	public Mock<IFileStorageService> MockFileStorageService { get; private set; }
	public Mock<IChatCompletionService> MockChatCompletionService { get; private set; }
	public Mock<ILogger<PolicyQuestionAnswering>> MockLogger { get; private set; }

	// Real instance (Kernel cannot be mocked as it's sealed)
	public Kernel Kernel { get; private set; }

	// Service instance
	public PolicyQuestionAnswering PolicyQuestionAnsweringSkill { get; private set; }

	public PolicyQuestionAnsweringFixture()
	{
		// Initialize mocks
		MockPolicyRepository = new Mock<IPolicyRepository>();
		MockExcelQuestionExtractor = new Mock<IExcelQuestionExtractor>();
		MockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
		MockFileStorageService = new Mock<IFileStorageService>();
		MockChatCompletionService = new Mock<IChatCompletionService>();
		MockLogger = new Mock<ILogger<PolicyQuestionAnswering>>();

		// Create a real Kernel instance with mocked chat completion service
		var kernelBuilder = Kernel.CreateBuilder();
		kernelBuilder.Services.AddSingleton(MockChatCompletionService.Object);
		Kernel = kernelBuilder.Build();

		// Create service instance
		PolicyQuestionAnsweringSkill = new PolicyQuestionAnswering(
			MockPolicyRepository.Object,
			MockExcelQuestionExtractor.Object,
			MockEmbeddingGenerator.Object,
			MockFileStorageService.Object,
			Kernel,
			MockLogger.Object
		);
	}

	public void Dispose()
	{
		// Cleanup if needed
	}
}
