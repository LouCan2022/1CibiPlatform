
using FluentAssertions;
using PhilSys.Features.GetLivenessKey;
using Test.BackendAPI.Infrastructure.PhilSys.Infrastracture;

namespace Test.BackendAPI.Modules.PhilSys.IntegrationTests
{
	public class GetLivenessKeyIntegrationTests : BaseIntegrationTest
	{
		public GetLivenessKeyIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
		{
		}

		[Fact]
		public async Task GetLivenessKey_ShouldReturnKey_WhenConfigured()
		{
			// Arrange
			var expectedKey = _configuration["PhilSys:LivenessSDKPublicKey"] = "HabaL5avPCryiszlRKNU7Q9xClqKEq5h2FWNLdMNEpo";

			var command = new GetLivenessKeyQueryRequest();

			// Act
			var result = await _sender.Send(command);

			// Assert
			result.LivenessKey.Should().Be(expectedKey);
		}

		[Fact]
		public async Task GetLivenessKey_ShouldReturnEmpty_WhenKeyIsNotConfigured()
		{
			// Arrange
			_configuration["PhilSys:LivenessSDKPublicKey"] = null;

			var command = new GetLivenessKeyQueryRequest();

			// Act
			var result = await _sender.Send(command);

			// Assert
			result.LivenessKey.Should().BeEmpty();
		}
	}
}
