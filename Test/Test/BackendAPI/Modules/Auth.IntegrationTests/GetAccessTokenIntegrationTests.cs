using System.Security.Cryptography;
using System.Text;
using Auth.Features.GetNewAccessToken;
using BuildingBlocks.Exceptions;
using FluentAssertions;
using Auth.Data.Entities;
using Test.BackendAPI.Infrastructure.Auth.Infrastructure;

namespace Test.BackendAPI.Modules.Auth.IntegrationTests;

public class GetAccessTokenIntegrationTests : BaseIntegrationTest
{
	public GetAccessTokenIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
	{
	}

	[Fact]
	public async Task GetNewAccessToken_ShouldThrowNotFound_WhenUserDoesNotExist()
	{
		// Arrange
		var nonExistentUserId = Guid.NewGuid();
		var refreshToken = "some-random-token";

		var command = new GetNewAccessTokenCommand(nonExistentUserId, refreshToken);

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Refresh Token is not found.");
	}

	[Fact]
	public async Task GetNewAccessToken_ShouldReturnOk_WhenRefreshTokenIsValid()
	{
		// Arrange - seed user and refresh token in DB
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = "refreshuser@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Refresh",
			LastName = "User",
			IsActive = true
		};
		_dbContext.AuthUsers.Add(user);

		var refreshToken = "valid-refresh-token";
		var hashed = ComputeSha256Base64(refreshToken);

		var authRefresh = new AuthRefreshToken
		{
			UserId = user.Id,
			TokenHash = hashed,
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddDays(7),
			IsActive = true
		};

		_dbContext.AuthRefreshToken.Add(authRefresh);
		await _dbContext.SaveChangesAsync();


		var command = new GetNewAccessTokenCommand(user.Id, refreshToken);

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.loginResponseWebDTO.Should().NotBeNull();
		result.loginResponseWebDTO.Access_token.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.refresh_token.Should().NotBeNullOrEmpty();
		result.loginResponseWebDTO.Token_type.Should().Be("bearer");
		result.loginResponseWebDTO.userId.Should().Be(user.Id.ToString());
		result.loginResponseWebDTO.expires_in.Should().BeGreaterThan(0);
	}


	[Fact]
	public async Task GetNewAccessToken_ShouldThrowNotFoundException_WhenUserIsInvalid()
	{
		// Arrange
		var olduUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
		var differentUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
		var user = new Authusers
		{
			Id = olduUserId,
			Email = "refreshuser2@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Refresh",
			LastName = "User",
			IsActive = true
		};
		_dbContext.AuthUsers.Add(user);

		var realToken = "real-refresh-token";
		var hashed = ComputeSha256Base64(realToken);

		var authRefresh = new AuthRefreshToken
		{
			UserId = olduUserId,
			TokenHash = hashed,
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddDays(7),
			IsActive = true
		};

		_dbContext.AuthRefreshToken.Add(authRefresh);
		await _dbContext.SaveChangesAsync();

		var command = new GetNewAccessTokenCommand(differentUserId, "invalid-token");

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Refresh Token is not found.");
	}


	[Fact]
	public async Task GetNewAccessToken_ShouldThrowUnauthorized_WhenRefreshTokenIsInvalid()
	{
		// Arrange - seed user with a refresh token, but provide a different token
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = "refreshuser2@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Refresh",
			LastName = "User",
			IsActive = true
		};
		_dbContext.AuthUsers.Add(user);

		var realToken = "real-refresh-token";
		var hashed = ComputeSha256Base64(realToken);

		var authRefresh = new AuthRefreshToken
		{
			UserId = user.Id,
			TokenHash = hashed,
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddDays(7),
			IsActive = true
		};

		_dbContext.AuthRefreshToken.Add(authRefresh);
		await _dbContext.SaveChangesAsync();

		var command = new GetNewAccessTokenCommand(user.Id, "invalid-token");

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid refresh token.");
	}



	private static string ComputeSha256Base64(string input)
	{
		using var sha256 = SHA256.Create();
		var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
		return Convert.ToBase64String(hash);
	}
}
