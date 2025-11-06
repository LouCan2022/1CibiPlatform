using System.Security.Cryptography;
using System.Text;
using Auth.DTO;
using Auth.Features.Logout;
using Auth.Data.Entities;
using FluentAssertions;
using Test.BackendAPI.Infrastructure;
using BuildingBlocks.Exceptions;

namespace Test.BackendAPI.Modules.Auth.IntegrationTests;

public class LogoutIntegrationTests : BaseIntegrationTest
{
	public LogoutIntegrationTests(IntegrationTestWebAppFactory factory) : base(factory)
	{
	}

	[Fact]
	public async Task Logout_ShouldReturnTrue_WhenUserExists()
	{
		// Arrange
		var user = await SeedUserDataWithRefreshToken();

		var logoutDto = new LogoutDTO(user.Id, "User initiated logout");

		var command = new LogoutCommand(logoutDto);

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.IsLoggedOut.Should().BeTrue();
	}

	[Fact]
	public async Task Logout_ShouldThrowBadRequest_WhenNoRefreshCookie()
	{
		// Arrange: seed user but do not set cookie and do not add refresh token
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = "nocookie@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "No",
			LastName = "Cookie",
			IsActive = true
		};
		_dbContext.AuthUsers.Add(user);
		await _dbContext.SaveChangesAsync();

		// ensure no cookie present
		var ctx = _httpContextAccessor.HttpContext!;
		ctx.Request.Headers.Remove("Cookie");

		var command = new LogoutCommand(new LogoutDTO(user.Id, "reason"));

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<BadRequestException>().WithMessage("Logout failed.");
	}

	[Fact]
	public async Task Logout_ShouldThrowNotFound_WhenUserHasNoRefreshRecord()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var user = new Authusers
		{
			Id = userId,
			Email = "norecord@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "No",
			LastName = "Record",
			IsActive = true
		};
		_dbContext.AuthUsers.Add(user);
		await _dbContext.SaveChangesAsync();

		//
		var refreshToken = "some-token-no-record";
		var cookieKey = _configuration["AuthWeb:AuthWebHttpCookieOnlyKey"] ?? "refreshKey";
		var ctx = _httpContextAccessor.HttpContext!;
		ctx.Request.Headers["Cookie"] = $"{cookieKey}={refreshToken}";

		var command = new LogoutCommand(new LogoutDTO(userId, "reason"));

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found.");
	}

	[Fact]
	public async Task Logout_ShouldThrowBadRequest_WhenRefreshTokenIsInvalid()
	{
		// Arrange
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = "invalidtoken@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Invalid",
			LastName = "Token",
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

		// set cookie with a different (invalid) token
		var invalidToken = "invalid-token";
		var cookieKey = _configuration["AuthWeb:AuthWebHttpCookieOnlyKey"] ?? "refreshKey";
		var ctx = _httpContextAccessor.HttpContext!;
		ctx.Request.Headers["Cookie"] = $"{cookieKey}={invalidToken}";

		var command = new LogoutCommand(new LogoutDTO(user.Id, "reason"));

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<BadRequestException>().WithMessage("Logout failed.");
	}

	private async Task<Authusers> SeedUserDataWithRefreshToken()
	{
		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = "logoutuser@example.com",
			PasswordHash = _passwordHasherService.HashPassword("p@ssw0rd!"),
			FirstName = "Test",
			LastName = "User",
			IsActive = true
		};
		_dbContext.AuthUsers.Add(user);

		var refreshToken = "logout-refresh-token";
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

		var ctx = _httpContextAccessor.HttpContext!;
		var cookieKey = _configuration["AuthWeb:AuthWebHttpCookieOnlyKey"] ?? "refreshKey";
		ctx.Request.Headers["Cookie"] = $"{cookieKey}={refreshToken}";

		return user;
	}

	private static string ComputeSha256Base64(string input)
	{
		using var sha256 = SHA256.Create();
		var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
		return Convert.ToBase64String(hash);
	}
}
