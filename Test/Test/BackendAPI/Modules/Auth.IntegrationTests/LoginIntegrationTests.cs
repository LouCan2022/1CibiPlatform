using Auth.Data.Entities;
using Auth.DTO;
using Auth.Features.Login;
using FluentAssertions;
using Test.BackendAPI.Infrastructure;

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
		// Arrange — only seed what THIS test needs
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

		var command = new LoginCommand(new LoginCred("john@example.com", "p@ssw0rd!"));

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.loginResponseDTO.UserName.Should().Be("john@example.com");
	}
}
