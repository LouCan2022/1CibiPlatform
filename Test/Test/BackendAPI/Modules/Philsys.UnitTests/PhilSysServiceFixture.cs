using PhilSys.Services;
using PhilSys.Data.Repository;
using BuildingBlocks.SharedServices.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;

namespace Test.BackendAPI.Modules.PhilSys.UnitTests.Fixture
{
	public class PhilSysServiceFixture : IDisposable
	{
		// Common mocks
		public Mock<IHashService> MockHashService { get; private set; }
		public Mock<ISecureToken> MockSecureToken { get; private set; }
		public Mock<IPhilSysRepository> MockPhilSysRepository { get; private set; }
		public Mock<IPhilSysResultRepository> MockPhilSysResultRepository { get; private set; }
		public Mock<IHttpClientFactory> MockHttpClientFactory { get; private set; }
		public Mock<IPhilSysService> MockPhilSysService { get; private set; }
		public Mock<HttpClient> MockHttpClient { get; private set; }

		// Loggers
		public Mock<ILogger<DeleteTransactionService>> MockDeleteTransactionLogger { get; private set; }
		public Mock<ILogger<GetLivenessKeyService>> MockGetLivenessKeyLogger { get; private set; }
		public Mock<ILogger<LivenessSessionService>> MockLivenessSessionLogger { get; private set; }
		public Mock<ILogger<PartnerSystemService>> MockPartnerSystemLogger { get; private set; }
		public Mock<ILogger<UpdateFaceLivenessSessionService>> MockUpdateFaceLivenessSessionLogger { get; private set; }
		public Mock<ILogger<PhilSysService>> MockPhilSysServiceLogger { get; private set; }

		// Configuration
		public IConfiguration Configuration { get; private set; }

		// Service instances
		public DeleteTransactionService DeleteTransactionService { get; private set; }
		public GetLivenessKeyService GetLivenessKeyService { get; private set; }
		public LivenessSessionService LivenessSessionService { get; private set; }
		public PartnerSystemService PartnerSystemService { get; private set; }
		public UpdateFaceLivenessSessionService UpdateFaceLivenessSessionService { get; private set; }
		public PhilSysService PhilSysService { get; private set; }
		public PhilSysServiceFixture()
		{
			// init mocks
			MockHashService = new Mock<IHashService>();
			MockPhilSysRepository = new Mock<IPhilSysRepository>();
			MockPhilSysResultRepository = new Mock<IPhilSysResultRepository>();
			MockHttpClientFactory = new Mock<IHttpClientFactory>();
			MockPhilSysService = new Mock<IPhilSysService>();
			MockSecureToken = new Mock<ISecureToken>();

			MockDeleteTransactionLogger = new Mock<ILogger<DeleteTransactionService>>();
			MockGetLivenessKeyLogger = new Mock<ILogger<GetLivenessKeyService>>();
			MockLivenessSessionLogger = new Mock<ILogger<LivenessSessionService>>();
			MockPartnerSystemLogger = new Mock<ILogger<PartnerSystemService>>();
			MockUpdateFaceLivenessSessionLogger = new Mock<ILogger<UpdateFaceLivenessSessionService>>();
			MockPhilSysServiceLogger = new Mock<ILogger<PhilSysService>>();

			// configuration values required by several services
			Configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string,string>("PhilSys:LivenessSessionExpiryInMinutes","5"),
					new KeyValuePair<string,string>("PhilSys:LivenessBaseUrl","http://localhost:5134"),
					new KeyValuePair<string,string>("PhilSys:ClientID","9ffe0ab6-1be1-47a8-bd3a-8560f1652a1a"),
					new KeyValuePair<string,string>("PhilSys:ClientSecret","YnQpGs34mdlH2bumAzzEhRc0pJXAjfcX8qBSDZyMtdiU4HDVwx4SAsIFLuLxHt51"),
					new KeyValuePair<string,string>("PhilSys:LivenessSDKPublicKey","eyJpdiI6Im9YTTRTTXpwbDF0ZlRvakFHRG1HTnc9PSIsInZhbHVlIjoiUlo3WFJmM1dZUEVSdmNNbDJrU3o2Zz09IiwibWFjIjoiZjJmNWQxN2M4ZjgxMDQ1NDE5MzYzNTU1ZWNiMzU0MDk3Y2ZkNjc5NDA1Y2VlOTViOTQ5NmJhMWIzN2NiMzIxZCIsInRhZyI6IiJ9"),
				})
				.Build();

			// create service instances using shared mocks
			DeleteTransactionService = new DeleteTransactionService(
				MockPhilSysRepository.Object,
				new Mock<ILogger<DeleteTransactionService>>().Object
			);

			GetLivenessKeyService = new GetLivenessKeyService(
				MockGetLivenessKeyLogger.Object,
				Configuration
			);

			LivenessSessionService = new LivenessSessionService(
				MockPhilSysRepository.Object,
				MockHashService.Object,
				MockLivenessSessionLogger.Object
			);

			PartnerSystemService = new PartnerSystemService(
				MockPartnerSystemLogger.Object,
				MockPhilSysRepository.Object,
				Configuration,
				MockHashService.Object,
				MockSecureToken.Object
			);

			PhilSysService = new PhilSysService(
				MockHttpClientFactory.Object,
				MockPhilSysServiceLogger.Object
				);

			UpdateFaceLivenessSessionService = new UpdateFaceLivenessSessionService(
				MockHttpClientFactory.Object,
				MockPhilSysRepository.Object,
				MockPhilSysResultRepository.Object,
				MockUpdateFaceLivenessSessionLogger.Object,
				MockPhilSysService.Object,
				Configuration
			);
		}
		public void Dispose()
		{
			// nothing to dispose currently
		}
	}
}
