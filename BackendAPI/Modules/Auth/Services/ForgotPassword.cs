namespace Auth.Services;

public class ForgotPassword : IForgotPassword
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<ForgotPassword> _logger;
	private readonly IEmailService _emailService;
	private readonly IConfiguration _configuration;
	private readonly ISecureToken _secureToken;
	private readonly IHashService _hashService;
	private readonly IPasswordHasherService _passwordHasherService;
	private readonly string _frontendBaseUrl;
	private readonly int _passwordTokenExpiryMinutes;

	public ForgotPassword(
		IAuthRepository authRepository,
		ILogger<ForgotPassword> logger,
		IEmailService emailService,
		IConfiguration configuration,
		ISecureToken secureToken,
		IHashService hashService,
		IPasswordHasherService passwordHasherService)
	{
		this._authRepository = authRepository;
		this._logger = logger;
		this._emailService = emailService;
		this._configuration = configuration;
		this._secureToken = secureToken;
		this._hashService = hashService;
		this._passwordHasherService = passwordHasherService;
		this._frontendBaseUrl = _configuration.GetValue<string>("FrontEndMetadata:ForgotPasswordUrl") ?? "";
		this._passwordTokenExpiryMinutes = _configuration.GetValue<int>("Email:PasswordTokenExpirationInMinutes");
	}

	public async Task<Guid> ForgotPasswordAsync(string email)
	{
		_logger.LogInformation("ForgotPassword called with email: {Email}", email);

		var user = await _authRepository.IsUserEmailExistAsync(email);

		if (user == null)
		{
			_logger.LogWarning("No user found with email: {Email}", email);
			throw new NotFoundException("Email not found.");
		}

		var secureToken = _secureToken.GenerateSecureToken();

		if (secureToken == null)
		{
			_logger.LogError("Failed to generate secure token for email: {Email}", email);
			throw new Exception("Failed to generate secure token.");
		}

		var hashedToken = _hashService.Hash(secureToken);

		var resetLink = $"{_frontendBaseUrl}/reset-password?token={System.Net.WebUtility.UrlEncode(hashedToken)}";

		var name = $"{user.FirstName} {user.LastName}";

		var emailBody = _emailService.SendPasswordResetBody(name, resetLink, this._passwordTokenExpiryMinutes);

		var isSent = await _emailService.SendEmailAsync(
			toEmail: email,
			subject: "Password Reset Request",
			body: emailBody
		);

		if (!isSent)
		{
			_logger.LogError("Failed to send password reset email to: {Email}", email);
			throw new Exception("Failed to send password reset email.");
		}

		_logger.LogInformation("Sent password reset email to: {Email}", email);

		var userDetailsPasswordToken = new PasswordResetToken
		{
			UserId = user.Id,
			TokenHash = hashedToken,
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddMinutes(_passwordTokenExpiryMinutes),
			IsUsed = false
		};

		var saveTokenResult = await _authRepository.SaveToResetPasswordToken(userDetailsPasswordToken);

		if (!saveTokenResult)
		{
			_logger.LogError("Failed to save password reset token for email: {Email}", email);
			throw new Exception("Failed to save password reset token.");
		}

		return user.Id;
	}

	public async Task<bool> IsTokenValid(string tokeHash)
	{
		var token = await _authRepository.GetUserTokenAsync(tokeHash);

		if (token == null || token.IsUsed || token.ExpiresAt < DateTime.UtcNow)
		{
			_logger.LogWarning("Invalid or expired token: {TokenHash}", tokeHash);
			return false;
		}

		return true;
	}

	public async Task<bool> ResetPasswordAsync(
		Guid id,
		string tokenHash,
		string newPassword)
	{
		var isTokenValid = this.IsTokenValid(tokenHash);

		if (!isTokenValid.Result)
		{
			_logger.LogWarning("Invalid or expired token for user ID: {UserId}", id);
			throw new UnauthorizedAccessException("Invalid or expired token.");
		}

		_logger.LogInformation("ResetPasswordAsync called for user ID: {UserId}", id);

		var userNewPassword = await _authRepository.GetRawUserAsync(id);

		if (userNewPassword == null)
		{
			_logger.LogWarning("No user found with ID: {UserId}", id);
			throw new NotFoundException("User not found.");
		}

		_logger.LogInformation("User found with ID: {UserId}", id);

		var newHashedPassword = _passwordHasherService.HashPassword(newPassword);

		userNewPassword.PasswordHash = newHashedPassword;

		var isUpdated = await _authRepository.UpdateAuthUserPassword(userNewPassword);

		if (!isUpdated)
		{
			_logger.LogError("Failed to update password for user ID: {UserId}", id);
			throw new Exception("Failed to update password.");
		}

		_logger.LogInformation("Password updated for user ID: {UserId}", id);

		var token = await _authRepository.GetUserTokenAsync(tokenHash);

		token.IsUsed = true;
		token.UsedAt = DateTime.UtcNow;

		var isTokenUpdated = await _authRepository.UpdatePasswordResetTokenAsUsedAsync(token);

		_logger.LogInformation("Marking token as used for user ID: {UserId}", id);

		if (!isTokenUpdated)
		{
			_logger.LogError("Failed to update password reset token for user ID: {UserId}", id);
			throw new Exception("Failed to update password reset token.");
		}

		_logger.LogInformation("Password updated successfully for user ID: {UserId}", id);

		return true;
	}
}
