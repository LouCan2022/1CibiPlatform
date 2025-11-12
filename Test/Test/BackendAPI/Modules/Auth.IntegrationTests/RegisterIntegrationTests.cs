using Auth.DTO;
using Auth.Features.Register;
using Auth.Data.Entities;
using FluentAssertions;
using Auth.Features.VerifyOtp;
using Test.BackendAPI.Infrastructure.Auth.Infrastructure;

namespace Test.BackendAPI.Modules.Auth.IntegrationTests;

public class RegisterIntegrationTests : BaseIntegrationTest
{
	public RegisterIntegrationTests(IntegrationTestWebAppFactory factory)
		: base(factory)
	{
	}

	[Fact]
	public async Task Register_ShouldReturnOtp_WhenRequestIsValid()
	{
		// Arrange
		var registerDto = new RegisterRequestDTO(
			Email: "uniqueuser@example.com",
			PasswordHash: "P@ssword1!",
			FirstName: "Unique",
			LastName: "User",
			MiddleName: string.Empty
		);

		var command = new RegisterRequestCommand(registerDto);

		// Act
		var result = await _sender.Send(command);

		// Assert
		result.Should().NotBeNull();
		result.otpVerificationResponse.Should().NotBeNull();
		result.otpVerificationResponse.Email.Should().Be(registerDto.Email);
		result.otpVerificationResponse.OtpId.Should().NotBe(Guid.Empty);
	}

	[Fact]
	public async Task Register_ShouldThrow_WhenEmailAlreadyExists()
	{
		// Arrange - seed existing user
		var existingUser = new OtpVerification
		{
			Email = "existinguser@example.com",
			PasswordHash = _passwordHasherService.HashPassword("Existing1!"),
			FirstName = "Existing",
			LastName = "User",
			IsUsed = true
		};
		await SeedOtpRecordAsync(existingUser);

		var registerDto = new RegisterRequestDTO(
			Email: existingUser.Email,
			PasswordHash: _passwordHasherService.HashPassword("Existing1!"),
			FirstName: "Duplicate",
			LastName: "User",
			MiddleName: string.Empty
		);

		var command = new RegisterRequestCommand(registerDto);

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("Email already in use.");
	}


	[Fact]
	public async Task VerifyOtp_ShouldRetrunTrue_WhenSuccessful()
	{
		// Arrange - seed OTP record
		var existingUser = new OtpVerification
		{
			Email = "verifyOTP@example.com",
			PasswordHash = _passwordHasherService.HashPassword("P@ssw0rd!"),
			FirstName = "Verify",
			LastName = "User",
			OtpCodeHash = _hashService.Hash("123456"),
			ExpiresAt = DateTime.UtcNow.AddMinutes(5),
		};
		await SeedOtpRecordAsync(existingUser);

		var otpRequestDto = new OtpRequestDTO(
			Email: existingUser.Email,
			Otp: "123456"
		);

		var command = new VerifyOtpCommand(otpRequestDto);

		// Act
		var result = await _sender.Send(command);


		// Assert
		result.Should().NotBeNull();
		result.IsVerified.Should().BeTrue();
	}

	[Fact]
	public async Task VerifyOtp_ShouldThrow_WhenOtpIsInvalid()
	{
		// Arrange
		var existingUser = new OtpVerification
		{
			Email = "verifyOTP@example.com",
			PasswordHash = _passwordHasherService.HashPassword("P@ssw0rd!"),
			FirstName = "Verify",
			LastName = "User",
			OtpCodeHash = _hashService.Hash("123456"),
			ExpiresAt = DateTime.UtcNow.AddMinutes(5),
		};

		await SeedOtpRecordAsync(existingUser);

		var otpRequestDto = new OtpRequestDTO(
			Email: existingUser.Email,
			Otp: "654321"
		);

		var command = new VerifyOtpCommand(otpRequestDto);

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert

		await act.Should().ThrowAsync<Exception>().WithMessage("Invalid OTP.");
	}


	[Fact]
	public async Task VerifyOtp_ShouldThrow_WhenEmailIsInvalid()
	{
		// Arrange
		var existingUser = new OtpVerification
		{
			Email = "verifyOTP@example.com",
			PasswordHash = _passwordHasherService.HashPassword("P@ssw0rd!"),
			FirstName = "Verify",
			LastName = "User",
			OtpCodeHash = _hashService.Hash("123456"),
			ExpiresAt = DateTime.UtcNow.AddMinutes(5),
		};

		await SeedOtpRecordAsync(existingUser);

		var otpRequestDto = new OtpRequestDTO(
			Email: "notExisting@example.com",
			Otp: "123454"
		);

		var command = new VerifyOtpCommand(otpRequestDto);

		// Act
		Func<Task> act = async () => { await _sender.Send(command); };

		// Assert
		await act.Should().ThrowAsync<Exception>().WithMessage("No OTP record found for this email.");
	}

	private async Task SeedOtpRecordAsync(OtpVerification otpRecord)
	{
		_dbContext.OtpVerification.Add(otpRecord);
		await _dbContext.SaveChangesAsync();
	}
}
