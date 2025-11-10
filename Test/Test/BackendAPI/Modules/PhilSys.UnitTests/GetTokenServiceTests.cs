using FluentAssertions;
using PhilSys.Services;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Test.BackendAPI.Modules.PhilSys.UnitTests.Fixture;

namespace Test.BackendAPI.Modules.PhilSys.UnitTests
{
	public class GetTokenServiceTests : IClassFixture<PhilSysServiceFixture>
	{
		private readonly PhilSysServiceFixture _fixture;
		public GetTokenServiceTests(PhilSysServiceFixture fixture)
		{
			_fixture = fixture;
		}


		[Fact]
		public async Task GetPhilsysTokenAsync_ShouldThrow_WhenRequestFails()
		{
			// Arrange
			var client_id = Guid.NewGuid().ToString();
			var client_secret = "YnQpGs34mdlH24234234EhRc0pJXAjQASDdASdjvihbujtuLxHt51";
			_fixture.MockHttpClientFactory.Setup(f => f.CreateClient("PhilSys")).Returns(() =>
			{
				var handlerStub = new DelegatingHandlerStub((request, ct) =>
				{
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
					{
						Content = new StringContent("{\"data\": null}", Encoding.UTF8, "application/json")
					});
				});
				return new HttpClient(handlerStub)
				{
					BaseAddress = new Uri("https://ws.everify.gov.ph/api/auth")
				};
			});
			var service = new GetTokenService(
							_fixture.MockHttpClientFactory.Object,
							_fixture.MockGetTokenLogger.Object
						);
			// Act
			Func<Task> act = async () => await service.GetPhilsysTokenAsync(client_id, client_secret);

			// Assert
			await act.Should().ThrowAsync<HttpRequestException>().WithMessage("PhilSys token request failed.");
		}

		[Fact]
		public async Task GetPhilsysTokenAsync_ShouldReturnToken_WhenResponseIsSuccessful()
		{
			// Arrange
			var client_id = Guid.NewGuid().ToString();
			var client_secret = "YnQpGs34mdlH24234234EhRc0pJXAjQASDdASdjvihbujtuLxHt51";
			var expectedToken = "fake-token";
			_fixture.MockHttpClientFactory.Setup(f => f.CreateClient("PhilSys")).Returns(() =>
			{
				var handlerStub = new DelegatingHandlerStub((request, ct) =>
				{
					return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
					{
						Content = JsonContent.Create(new { data = new { access_token = expectedToken } })
					});
				});
				return new HttpClient(handlerStub)
				{
					BaseAddress = new Uri("https://ws.everify.gov.ph/api/auth")
				};
			});
			var service = new GetTokenService(
							_fixture.MockHttpClientFactory.Object,
							_fixture.MockGetTokenLogger.Object
						);

			// Act
			var token = await service.GetPhilsysTokenAsync(client_id, client_secret);

			// Assert
			token.Should().Be(expectedToken);
		}
	}
}
