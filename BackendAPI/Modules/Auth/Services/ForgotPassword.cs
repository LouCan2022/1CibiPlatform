namespace Auth.Services;

public class ForgotPassword : IForgotPassword
{
	private readonly IAuthRepository _authRepository;
	private readonly ILogger<ForgotPassword> _logger;
	private readonly IEmailService _emailService;
	private readonly IConfiguration _configuration;
	private readonly ISecureToken _secureToken;
	private readonly IHashService _hashService;
	private readonly string _frontendBaseUrl;
	private readonly int _passwordTokenExpiryMinutes;

	public ForgotPassword(
		IAuthRepository authRepository,
		ILogger<ForgotPassword> logger,
		IEmailService emailService,
		IConfiguration configuration,
		ISecureToken secureToken,
		IHashService hashService)
	{
		this._authRepository = authRepository;
		this._logger = logger;
		this._emailService = emailService;
		this._configuration = configuration;
		this._secureToken = secureToken;
		this._hashService = hashService;
		this._frontendBaseUrl = _configuration.GetValue<string>("FrontendBaseUrl:Url") ?? "";
		this._passwordTokenExpiryMinutes = _configuration.GetValue<int>("Email:PasswordTokenExpirationInMinutes");
	}

	public async Task<Guid> ForgotPasswordAsync(string email)
	{
		_logger.LogInformation("ForgotPassword called with email: {Email}", email);

		var user = await _authRepository.IsUserEmailExistAsync(email);

		if (user == null)
		{
			_logger.LogWarning("No user found with email: {Email}", email);
			throw new ArgumentException("Email not found.");
		}

		var secureToken = _secureToken.GenerateSecureToken();

		if (secureToken == null)
		{
			_logger.LogError("Failed to generate secure token for email: {Email}", email);
			throw new Exception("Failed to generate secure token.");
		}

		var hashedToken = _hashService.Hash(secureToken);

		var resetLink = $"{_frontendBaseUrl}/reset-password?token={System.Net.WebUtility.UrlEncode(secureToken)}&email={System.Net.WebUtility.UrlEncode(email)}";

		var name = $"{user.FirstName} {user.LastName}";

		var emailBody = _emailService.SendPasswordResetBody(name, resetLink);

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

	public Task<bool> ResetPasswordAsync(Guid id, string otp, string newPassword)
	{
		throw new NotImplementedException();
	}
}
