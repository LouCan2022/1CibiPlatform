using Auth.Data.Entities;
using Auth.DTO;
using Auth.Features.Login;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using Test.BackendAPI.Infrastructure;
using static Auth.Features.LoginWeb.LoginWebHandler;

namespace Test.BackendAPI.Modules.Auth.IntegrationTests;

public class LoginIntegrationTests : BaseIntegrationTest
{
	public LoginIntegrationTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task Login_ShouldReturnSuccess_WhenCredentialsAreCorrect()
	{
		// Arrange
		await SeedUserData();

		var command = new LoginCommand(new LoginCred("john@example.com", "p@ssw0rd!"));

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.loginResponseDTO.UserName.Should().Be("john@example.com");
	}


	[Fact]
	public async Task Login_ShouldReturnNotFound_WhenCredentialsAreIncorrect()
	{
		// Arrange
		await SeedUserData();

		var command = new LoginCommand(new LoginCred("john@example.com", "wrongpassword"));

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Invalid username or password.");
	}


	[Fact]
	public async Task LoginWeb_ShouldReturnSuccess_WhenCredentialsAreCorrect()
	{
		// Arrange
		await SeedUserData();

		var command = new LoginWebCommand(new LoginWebCred("john@example.com", "p@ssw0rd!", false));

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.loginResponseWebDTO.Should().NotBeNull();
		result.loginResponseWebDTO.Access_token.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.userId.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.refresh_token.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.Token_type.Should().Be("bearer");
		result.loginResponseWebDTO.expires_in.Should().BeGreaterThan(0);
	}


	[Fact]
	public async Task LoginWeb_ShouldReturnNotFound_WhenCredentialsAreCorrect()
	{
		// Arrange
		await SeedUserData();
		var command = new LoginWebCommand(new LoginWebCred("john@example.com", "wrongpassword", false));

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Invalid username or password.");
	}


	private async Task SeedUserData()
	{
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = "john@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Admin",
			LastName = ""
		};
		_dbContext.AuthUsers.Add(user);
		await _dbContext.SaveChangesAsync();
	}
}
