using Auth.Service;
using FluentAssertions;

namespace Test.BackendAPI.Modules.Auth.UnitTests;

public class PasswordHasherServiceTests
{
	private readonly IPasswordHasherService _passwordHasherService;
	public PasswordHasherServiceTests()
	{
		_passwordHasherService = new PasswordHasherService();
	}
	[Fact]
	public void HashPassword_ShouldReturnDifferentHashes_ForSamePassword()
	{
		// Arrange
		var password = "SecureP@ssw0rd!";
		// Act
		var hash1 = _passwordHasherService.HashPassword(password);
		var hash2 = _passwordHasherService.HashPassword(password);
		// Assert
		hash1.Should().NotBe(hash2, "Hashes for the same password should be different due to unique salts");
	}
	[Fact]
	public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
	{
		// Arrange
		var password = "SecureP@ssw0rd!";
		var hashedPassword = _passwordHasherService.HashPassword(password);
		// Act
		var isValid = _passwordHasherService.VerifyPassword(hashedPassword, password);
		// Assert
		isValid.Should().BeTrue("The provided password should match the hashed password");
	}
	[Fact]
	public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
	{
		// Arrange
		var password = "SecureP@ssw0rd!";
		var wrongPassword = "WrongP@ssw0rd!";
		var hashedPassword = _passwordHasherService.HashPassword(password);
		// Act
		var isValid = _passwordHasherService.VerifyPassword(hashedPassword, wrongPassword);
		// Assert
		isValid.Should().BeFalse("The provided password should not match the hashed password");
	}

}
