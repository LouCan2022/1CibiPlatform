using Moq;
using FluentAssertions;
using Test.BackendAPI.Modules.Auth.UnitTests.Fixture;
using Auth.Data.Entities;
using BuildingBlocks.Exceptions;

namespace Test.BackendAPI.Modules.Auth.UnitTests;

public class ForgotPasswordServiceTests : IClassFixture<AuthServiceFixture>
{
	private readonly AuthServiceFixture _fixture;

	public ForgotPasswordServiceTests(AuthServiceFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task ForgotPasswordAsync_ShouldThrow_WhenUserNotFound()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		_fixture.MockAuthRepository.Setup(x => x.IsUserEmailExistAsync(It.IsAny<string>())).ReturnsAsync((Authusers?)null);

		// Act
		Func<Task> act = async () => await service.ForgotPasswordAsync("missing@example.com");

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("Email not found.");
	}

	[Fact]
	public async Task ForgotPasswordAsync_ShouldThrow_WhenGenerateSecureTokenFails()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var user = new Authusers { Id = Guid.NewGuid(), FirstName = "F", LastName = "L", Email = "ok@example.com" };
		_fixture.MockAuthRepository.Setup(x => x.IsUserEmailExistAsync(user.Email)).ReturnsAsync(user);
		_fixture.MockSecureToken.Setup(x => x.GenerateSecureToken()).Returns((string?)null);

		// Act
		Func<Task> act = async () => await service.ForgotPasswordAsync(user.Email);

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("Failed to generate secure token.");
	}

	[Fact]
	public async Task ForgotPasswordAsync_ShouldThrow_WhenEmailSendFails()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var user = new Authusers { Id = Guid.NewGuid(), FirstName = "F", LastName = "L", Email = "ok@example.com" };
		_fixture.MockAuthRepository.Setup(x => x.IsUserEmailExistAsync(user.Email)).ReturnsAsync(user);
		_fixture.MockSecureToken.Setup(x => x.GenerateSecureToken()).Returns("secure");
		_fixture.MockHashService.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");
		_fixture.MockEmailService.Setup(x => x.SendPasswordResetBody(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("body");
		_fixture.MockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(false);

		// Act
		Func<Task> act = async () => await service.ForgotPasswordAsync(user.Email);

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("Failed to send password reset email.");
	}

	[Fact]
	public async Task ForgotPasswordAsync_ShouldThrow_WhenSaveTokenFails()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var user = new Authusers { Id = Guid.NewGuid(), FirstName = "F", LastName = "L", Email = "ok@example.com" };
		_fixture.MockAuthRepository.Setup(x => x.IsUserEmailExistAsync(user.Email)).ReturnsAsync(user);
		_fixture.MockSecureToken.Setup(x => x.GenerateSecureToken()).Returns("secure");
		_fixture.MockHashService.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed");
		_fixture.MockEmailService.Setup(x => x.SendPasswordResetBody(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns("body");
		_fixture.MockEmailService.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
		_fixture.MockAuthRepository.Setup(x => x.SaveToResetPasswordToken(It.IsAny<PasswordResetToken>())).ReturnsAsync(false);

		// Act
		Func<Task> act = async () => await service.ForgotPasswordAsync(user.Email);

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("Failed to save password reset token.");
	}

	[Fact]
	public async Task ResetPasswordAsync_ShouldThrow_WhenTokenInvalid()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var userId = Guid.NewGuid();
		var tokenHash = "invalidtoken";
		_fixture.MockAuthRepository.Setup(x => x.GetUserTokenAsync(tokenHash)).ReturnsAsync((PasswordResetToken?)null);

		// Act
		Func<Task> act = async () => await service.ResetPasswordAsync(userId, tokenHash, "newpass");

		// Assert
		await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("Invalid or expired token.");
	}

	[Fact]
	public async Task ResetPasswordAsync_ShouldThrow_WhenUserNotFound()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var userId = Guid.NewGuid();
		var tokenHash = "validtoken";
		var token = new PasswordResetToken { UserId = userId, IsUsed = false, ExpiresAt = DateTime.UtcNow.AddMinutes(10), TokenHash = tokenHash };
		_fixture.MockAuthRepository.Setup(x => x.GetUserTokenAsync(tokenHash)).ReturnsAsync(token);
		_fixture.MockAuthRepository.Setup(x => x.GetRawUserAsync(userId)).ReturnsAsync((Authusers?)null);

		// Act
		Func<Task> act = async () => await service.ResetPasswordAsync(userId, tokenHash, "newpass");

		// Assert
		await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found.");
	}

	[Fact]
	public async Task ResetPasswordAsync_ShouldThrow_WhenUpdatePasswordFails()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var userId = Guid.NewGuid();
		var tokenHash = "validtoken";
		var token = new PasswordResetToken { UserId = userId, IsUsed = false, ExpiresAt = DateTime.UtcNow.AddMinutes(10), TokenHash = tokenHash };
		var user = new Authusers { Id = userId, Email = "e@e.com", FirstName = "F", LastName = "L" };
		_fixture.MockAuthRepository.Setup(x => x.GetUserTokenAsync(tokenHash)).ReturnsAsync(token);
		_fixture.MockAuthRepository.Setup(x => x.GetRawUserAsync(userId)).ReturnsAsync(user);
		_fixture.MockPasswordHasherService.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("newhashed");
		_fixture.MockAuthRepository.Setup(x => x.UpdateAuthUserPassword(It.IsAny<Authusers>())).ReturnsAsync(false);

		// Act
		Func<Task> act = async () => await service.ResetPasswordAsync(userId, tokenHash, "newpass");

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("Failed to update password.");
	}

	[Fact]
	public async Task ResetPasswordAsync_ShouldThrow_WhenUpdateTokenAsUsedFails()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var userId = Guid.NewGuid();
		var tokenHash = "validtoken";
		var token = new PasswordResetToken { UserId = userId, IsUsed = false, ExpiresAt = DateTime.UtcNow.AddMinutes(10), TokenHash = tokenHash };
		var user = new Authusers { Id = userId, Email = "e@e.com", FirstName = "F", LastName = "L" };
		_fixture.MockAuthRepository.Setup(x => x.GetUserTokenAsync(tokenHash)).ReturnsAsync(token);
		_fixture.MockAuthRepository.Setup(x => x.GetRawUserAsync(userId)).ReturnsAsync(user);
		_fixture.MockPasswordHasherService.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("newhashed");
		_fixture.MockAuthRepository.Setup(x => x.UpdateAuthUserPassword(It.IsAny<Authusers>())).ReturnsAsync(true);
		_fixture.MockAuthRepository.Setup(x => x.UpdatePasswordResetTokenAsUsedAsync(It.IsAny<PasswordResetToken>())).ReturnsAsync(false);

		// Act
		Func<Task> act = async () => await service.ResetPasswordAsync(userId, tokenHash, "newpass");

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("Failed to update password reset token.");
	}

	[Fact]
	public async Task ResetPasswordAsync_ShouldReturnTrue_WhenSuccessful()
	{
		// Arrange
		var service = _fixture.ForgotPasswordService;
		var userId = Guid.NewGuid();
		var tokenHash = "validtoken";
		var token = new PasswordResetToken { UserId = userId, IsUsed = false, ExpiresAt = DateTime.UtcNow.AddMinutes(10), TokenHash = tokenHash };
		var user = new Authusers { Id = userId, Email = "e@e.com", FirstName = "F", LastName = "L" };
		_fixture.MockAuthRepository.Setup(x => x.GetUserTokenAsync(tokenHash)).ReturnsAsync(token);
		_fixture.MockAuthRepository.Setup(x => x.GetRawUserAsync(userId)).ReturnsAsync(user);
		_fixture.MockPasswordHasherService.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("newhashed");
		_fixture.MockAuthRepository.Setup(x => x.UpdateAuthUserPassword(It.IsAny<Authusers>())).ReturnsAsync(true);
		_fixture.MockAuthRepository.Setup(x => x.UpdatePasswordResetTokenAsUsedAsync(It.IsAny<PasswordResetToken>())).ReturnsAsync(true);

		// Act
		var result = await service.ResetPasswordAsync(userId, tokenHash, "newpass");

		// Assert
		result.Should().BeTrue();
	}
}
