namespace Auth.Services;

public class RegisterService : IRegisterService
{
	private readonly IEmailService _emailService;
	private readonly IPasswordHasherService _passwordHasherService;
	private readonly ILogger<RegisterService> _logger;
	private readonly IAuthRepository _authRepository;
	private readonly IHashService _hashService;
	private readonly IOtpService _otpService;
	private readonly IConfiguration _configuration;
	private readonly int _otpExpiryMinutes;

	public RegisterService(
		IEmailService emailService,
		IPasswordHasherService passwordHasherService,
		IAuthRepository authRepository,
		IHashService hashService,
		IOtpService otpService,
		IConfiguration configuration,
		ILogger<RegisterService> logger)
	{
		this._emailService = emailService;
		this._passwordHasherService = passwordHasherService;
		this._logger = logger;
		this._authRepository = authRepository;
		this._hashService = hashService;
		this._otpService = otpService;
		this._configuration = configuration;

		this._otpExpiryMinutes = int.Parse(_configuration["Email:OtpExpirationInMinutes"] ?? "15");

	}

	public async Task<OtpVerificationResponse> RegisterAsync(RegisterRequestDTO registerRequestDTO)
	{

		_logger.LogInformation("Starting registration process for email: {Email}", registerRequestDTO.Email);

		var isUserEmailExist = await _authRepository.IsUserEmailExistInOtpVerificationAsync(registerRequestDTO.Email, true);

		if (isUserEmailExist != null)
		{
			_logger.LogWarning("Email already in use: {Email}", registerRequestDTO.Email);
			throw new Exception("Email already in use.");
		}

		_logger.LogInformation("Email is available: {Email}", registerRequestDTO.Email);

		var otp = _otpService.GenerateOtp();

		_logger.LogInformation("Generated OTP for email: {Email}", registerRequestDTO.Email);

		if (otp == null)
		{
			_logger.LogError("Failed to generate OTP for email: {Email}", registerRequestDTO.Email);
			throw new Exception("Failed to generate OTP.");
		}


		var HashOTP = _hashService.Hash(otp);

		_logger.LogInformation("Hashed OTP for email: {Email}", registerRequestDTO.Email);

		if (HashOTP == null)
		{
			_logger.LogError("Failed to hash OTP for email: {Email}", registerRequestDTO.Email);
			throw new Exception("Failed to hash OTP.");
		}

		var name = $"{registerRequestDTO.FirstName} {registerRequestDTO.LastName}";

		var otpBody = await _emailService.SendOtpBody(registerRequestDTO.Email, name, otp);

		var isSent = await _emailService.SendEmailAsync(
			toEmail: registerRequestDTO.Email,
			subject: "Your OTP Code",
			body: otpBody
		);

		if (!isSent)
		{
			_logger.LogError("Failed to send OTP email to: {Email}", registerRequestDTO.Email);
			throw new Exception("Failed to send OTP email.");
		}

		_logger.LogInformation("Sent OTP email to: {Email}", registerRequestDTO.Email);


		var user = new OtpVerification
		{
			Email = registerRequestDTO.Email,
			OtpId = Guid.NewGuid(),
			FirstName = registerRequestDTO.FirstName,
			LastName = registerRequestDTO.LastName,
			MiddleName = registerRequestDTO.MiddleName!,
			PasswordHash = _passwordHasherService.HashPassword(registerRequestDTO.PasswordHash),
			OtpCodeHash = HashOTP,
			IsVerified = false,
			IsUsed = false,
			AttemptCount = 0,
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddMinutes(_otpExpiryMinutes)
		};

		var otpUser = await _authRepository.InsertOtpVerification(user);

		_logger.LogInformation("Stored OTP verification record for email: {Email}", registerRequestDTO.Email);

		if (!otpUser)
		{
			_logger.LogError("Failed to store OTP verification record for email: {Email}", registerRequestDTO.Email);
			throw new Exception("Failed to store OTP verification record.");
		}

		var userOtpResponse = user.Adapt<OtpVerificationResponse>();

		return userOtpResponse;
	}


	public async Task<bool> VerifyOtpAsync(string email, string otp)
	{
		_logger.LogInformation("Starting OTP verification for email: {Email}", email);

		var existingOtpRecord = await _authRepository.IsUserEmailExistInOtpVerificationAsync(email, false);

		if (existingOtpRecord == null)
		{
			_logger.LogWarning("No OTP record found for email: {Email}", email);
			throw new Exception("No OTP record found for this email.");
		}

		var hashOtp = _hashService.Hash(otp);

		var isOtpValid = _hashService.Verify(hashOtp, existingOtpRecord.OtpCodeHash);

		if (!isOtpValid)
		{
			_logger.LogWarning("Invalid OTP provided for email: {Email}", email);
			existingOtpRecord.AttemptCount += 1;
			await _authRepository.UpdateVerificationCodeAsync(existingOtpRecord);
			throw new Exception("Invalid OTP");
		}

		if (existingOtpRecord.IsUsed)
		{
			_logger.LogWarning("OTP already used for email: {Email}", email);
			throw new Exception("OTP already used.");
		}

		if (DateTime.UtcNow > existingOtpRecord.ExpiresAt)
		{
			_logger.LogWarning("OTP expired for email: {Email}", email);
			await this.ResendOtpAsync(existingOtpRecord);
			throw new InvalidOperationException("Your OTP has expired. A new code has been sent to your email.");
		}

		existingOtpRecord.IsVerified = true;
		existingOtpRecord.IsUsed = true;
		existingOtpRecord.VerifiedAt = DateTime.UtcNow;
		existingOtpRecord.AttemptCount += 1;

		var isUpdated = await _authRepository.UpdateVerificationCodeAsync(existingOtpRecord);

		if (!isUpdated)
		{
			_logger.LogError("Failed to update OTP record for email: {Email}", email);
			throw new Exception("Failed to update OTP record.");
		}

		var user = new Authusers
		{
			Id = Guid.NewGuid(),
			Email = existingOtpRecord.Email,
			PasswordHash = existingOtpRecord.PasswordHash,
			FirstName = existingOtpRecord.FirstName,
			LastName = existingOtpRecord.LastName,
			MiddleName = existingOtpRecord.MiddleName,
		};

		var isSuccess = await _authRepository.SaveUserAsync(user);

		if (!isSuccess)
		{
			_logger.LogError("Failed to save user record for email: {Email}", email);
			throw new Exception("Failed to save user record.");
		}

		_logger.LogInformation("Successfully verified OTP for email: {Email}", email);

		return true;
	}

	public async Task<bool> ManualResendOtpCodeAsync(Guid userId, string email)
	{
		var otpVerification = await _authRepository.IsUserEmailExistInOtpVerificationAsync(email, false);

		if (otpVerification == null)
		{
			_logger.LogWarning("No OTP record found for email: {Email}", email);
			throw new Exception("No OTP record found for this email.");
		}

		var user = new OtpVerificationRequestDTO(otpVerification.OtpId, otpVerification.Email);

		var isOtpValid = await _authRepository.OtpVerificationUserData(user);

		if (isOtpValid == null)
		{
			_logger.LogWarning("No OTP record found for email: {Email}", otpVerification.Email);
			throw new Exception("No OTP record found for this email.");
		}

		var userDetail = await _authRepository.IsUserEmailExistInOtpVerificationAsync(otpVerification.Email, false);

		if (userDetail == null)
		{
			_logger.LogWarning("No OTP record found for email: {Email}", otpVerification.Email);
			throw new Exception("No OTP record found for this email.");
		}

		var isSent = await ResendOtpAsync(otpVerification);

		if (!isSent)
		{
			_logger.LogError("Failed to resend OTP email to: {Email}", otpVerification.Email);
			throw new Exception("Failed to resend OTP email.");
		}

		_logger.LogInformation("Resent OTP email to: {Email}", otpVerification.Email);

		return isSent;
	}

	public async Task<bool> ResendOtpAsync(OtpVerification otpVerification)
	{
		var otp = _otpService.GenerateOtp();

		_logger.LogInformation("Generated OTP for email: {Email}", otpVerification.Email);

		otpVerification.OtpCodeHash = _hashService.Hash(otp);

		otpVerification.ExpiresAt = DateTime.UtcNow.AddMinutes(_otpExpiryMinutes);

		var isUpdated = await _authRepository.UpdateVerificationCodeAsync(otpVerification);

		if (!isUpdated)
		{
			_logger.LogError("Failed to update OTP record for email: {Email}", otpVerification.Email);
			throw new Exception("Failed to update OTP record.");
		}


		if (otp == null)
		{
			_logger.LogError("Failed to generate OTP for email: {Email}", otpVerification.Email);
			throw new Exception("Failed to generate OTP.");
		}


		var HashOTP = _hashService.Hash(otp);

		_logger.LogInformation("Hashed OTP for email: {Email}", otpVerification.Email);

		if (HashOTP == null)
		{
			_logger.LogError("Failed to hash OTP for email: {Email}", otpVerification.Email);
			throw new Exception("Failed to hash OTP.");
		}

		var name = $"{otpVerification.FirstName} {otpVerification.LastName}";

		var otpBody = await _emailService.SendOtpBody(otpVerification.Email, name, otp);

		var isSent = await _emailService.SendEmailAsync(
			toEmail: otpVerification.Email,
			subject: "Your OTP Code",
			body: otpBody
		);

		if (!isSent)
		{
			_logger.LogError("Failed to send OTP email to: {Email}", otpVerification.Email);
			throw new Exception("Failed to send OTP email.");
		}

		_logger.LogInformation("Sent OTP email to: {Email}", otpVerification.Email);

		return true;
	}

	public async Task<bool> IsOtpSessionValidAsync(Guid userId, string email)
	{
		_logger.LogInformation("Checking if OTP is valid for email: {Email}", email);


		var userDetail = new OtpVerificationRequestDTO(userId, email);

		var existingOtpRecord = await _authRepository.OtpVerificationUserData(userDetail);

		if (existingOtpRecord == null)
		{
			_logger.LogWarning("No OTP record found for email: {Email}", email);
			return false;
		}

		if (existingOtpRecord.IsUsed)
		{
			_logger.LogWarning("OTP already used for email: {Email}", email);
			return false;
		}

		return true;

	}


}
