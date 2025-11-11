using Moq;
using FluentAssertions;
using Test.BackendAPI.Modules.Auth.UnitTests.Fixture;
using Auth.DTO;
using BuildingBlocks.Exceptions;

namespace Test.BackendAPI.Modules.Auth.UnitTests;

public class LoginServiceTests : IClassFixture<AuthServiceFixture>
{
	private readonly AuthServiceFixture _fixture;

	public LoginServiceTests(AuthServiceFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
	{
		// Arrange
		var service = _fixture.LoginService;
		_fixture.MockAuthRepository.Setup(x => x.GetUserDataAsync(It.IsAny<LoginWebCred>()))
		.ReturnsAsync((LoginDTO?)null);

		// Act
		Func<Task> act = async () => await service.LoginAsync(new LoginCred("bad", "bad"));

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Invalid username or password.");
	}

	[Fact]
	public async Task LoginAsync_ShouldThrow_WhenPasswordInvalid()
	{
		// Arrange
		var service = _fixture.LoginService;
		var loginDto = new LoginDTO(Guid.NewGuid(), "hash", "email@example.com", "F", "L", null, new List<int> { 1 }, new List<List<int>> { new List<int> { 1 } }, new List<int> { 1 });
		_fixture.MockAuthRepository.Setup(x => x.GetUserDataAsync(It.IsAny<LoginWebCred>())).ReturnsAsync(loginDto);
		_fixture.MockPasswordHasherService.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

		// Act
		Func<Task> act = async () => await service.LoginAsync(new LoginCred("user", "wrong"));

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Invalid username or password.");
	}

	[Fact]
	public async Task LoginAsync_ShouldReturnResponse_WhenSuccessful()
	{
		// Arrange
		var service = _fixture.LoginService;
		var loginDto = new LoginDTO(Guid.NewGuid(), "hash", "email@example.com", "F", "L", null, new List<int> { 1 }, new List<List<int>> { new List<int> { 1 } }, new List<int> { 1 });
		_fixture.MockAuthRepository.Setup(x => x.GetUserDataAsync(It.IsAny<LoginWebCred>())).ReturnsAsync(loginDto);
		_fixture.MockPasswordHasherService.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
		_fixture.MockJwtService.Setup(x => x.GetAccessToken(It.IsAny<LoginDTO>())).Returns("token");

		// Act
		var resp = await service.LoginAsync(new LoginCred("user", "pass"));

		// Assert
		resp.Should().NotBeNull();
		resp.Access_token.Should().Be("token");
	}
}
