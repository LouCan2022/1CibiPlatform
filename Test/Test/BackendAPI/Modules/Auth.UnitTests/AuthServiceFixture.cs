using Auth.Services;
using Auth.Data.Repository;
using Auth.Service;
using BuildingBlocks.SharedServices.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Configuration;

namespace Test.BackendAPI.Modules.Auth.UnitTests.Fixture
{
	public class AuthServiceFixture : IDisposable
	{
		public Mock<IEmailService> MockEmailService { get; private set; }
		public Mock<IPasswordHasherService> MockPasswordHasherService { get; private set; }
		public Mock<IAuthRepository> MockAuthRepository { get; private set; }
		public Mock<IHashService> MockHashService { get; private set; }
		public Mock<IOtpService> MockOtpService { get; private set; }
		public IConfiguration Configuration { get; private set; }
		public Mock<ILogger<RegisterService>> MockLogger { get; private set; }

		public RegisterService RegisterService { get; private set; }

		public AuthServiceFixture()
		{
			MockEmailService = new Mock<IEmailService>();
			MockPasswordHasherService = new Mock<IPasswordHasherService>();
			MockAuthRepository = new Mock<IAuthRepository>();
			MockHashService = new Mock<IHashService>();
			MockOtpService = new Mock<IOtpService>();
			MockLogger = new Mock<ILogger<RegisterService>>();

			// provide a real IConfiguration with the needed value because GetValue<T> is an extension method
			Configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("Email:OtpExpirationInMinutes", "10") })
				.Build();

			RegisterService = new RegisterService(
				MockEmailService.Object,
				MockPasswordHasherService.Object,
				MockAuthRepository.Object,
				MockHashService.Object,
				MockOtpService.Object,
				Configuration,
				MockLogger.Object);
		}

		public void Dispose()
		{

		}
	}
}
