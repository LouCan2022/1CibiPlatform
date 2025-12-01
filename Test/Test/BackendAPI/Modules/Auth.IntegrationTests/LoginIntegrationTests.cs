using Auth.Data.Entities;
using Auth.DTO;
using Auth.Features.Login;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using Test.BackendAPI.Infrastructure.Auth.Infrastructure;
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

		var command = new LoginCommand("john@example.com", "p@ssw0rd!");

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.loginResponseDTO.email.Should().Be("john@example.com");
		result.loginResponseDTO.userId.Should().NotBeEmpty();
		result.loginResponseDTO.name.Should().Be("Admin Admin Admin");
		result.loginResponseDTO.access_token.Should().NotBeNullOrEmpty();
		result.loginResponseDTO.token_type.Should().Be("bearer");

	}


	[Fact]
	public async Task Login_ShouldReturnNotFound_WhenCredentialsAreIncorrect()
	{
		// Arrange
		await SeedUserData();

		var command = new LoginCommand("john@example.com", "wrongpassword");

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
		result.loginResponseWebDTO.AccessToken.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.UserId.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.RefreshToken.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.TokenType.Should().Be("bearer");
		result.loginResponseWebDTO.ExpiresIn.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task LoginWeb_ShouldUnauthorizedAccessException_WhenUserHasNoDesignatedApplication()
	{
		// Arrange
		await SeedUserOnlyData();

		var command = new LoginWebCommand(new LoginWebCred("john@example.com", "p@ssw0rd!", false));

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Your account has no assigned application. Please contact an administrator for assistance.");
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
		var userId = Guid.NewGuid();

		var user = new Authusers
		{
			Id = userId,
			Email = "john@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Admin",
			MiddleName = "Admin",
			LastName = "Admin"
		};

		var userRole = new List<AuthRole>
			{
				new AuthRole
				{
					RoleName = "SuperAdmin",
					Description = "Super Admin"
				},
				new AuthRole
				{
					RoleName = "Admin",
					Description = "Administrator Role"
				},
				new AuthRole
				{
					RoleName = "User",
					Description = "User Role"
				}
			};


		var submenu = new List<AuthSubMenu>
			{
				new AuthSubMenu
				{
					SubMenuName = "CNX Dashboard",
					Description = "List of Subjects"
				},
				new AuthSubMenu
				{
					SubMenuName = "IDV",
					Description = "Philsys IDV"
				}
			};

		var authapplication = new List<AuthApplication>
			{
				new AuthApplication
				{
					AppName = "CNX",
					Description = "Concentrix API"
				},
				new AuthApplication
				{
					AppName = "Philsys",
					Description = "IDV"
				}
			};

		var authUserRole = new List<AuthUserAppRole>
			{
				new AuthUserAppRole
				{
					UserId = userId,
					AppId = 1,
					Submenu = 1,
					RoleId = 1,
					AssignedBy = userId
				},
				new AuthUserAppRole
				{
					UserId = userId,
					AppId = 2,
					Submenu= 2,
					RoleId = 2,
					AssignedBy = userId
				}
			};

		_dbContext.AuthUsers.Add(user);
		_dbContext.AuthApplications.AddRange(authapplication);
		_dbContext.AuthRoles.AddRange(userRole);
		_dbContext.AuthSubmenu.AddRange(submenu);
		_dbContext.AuthUserAppRoles.AddRange(authUserRole);

		await _dbContext.SaveChangesAsync();
	}



	private async Task SeedUserOnlyData()
	{
		var userId = Guid.NewGuid();

		var user = new Authusers
		{
			Id = userId,
			Email = "john@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Admin",
			MiddleName = "Admin",
			LastName = "Admin"
		};

		_dbContext.AuthUsers.Add(user);

		await _dbContext.SaveChangesAsync();
	}
}
