namespace Auth.Services;

public class RegisterService : IRegisterService
{
	private readonly IEmailService _emailService;
	private readonly IPasswordHasherService _passwordHasherService;
	private readonly ILogger<RegisterService> _logger;
	private readonly IAuthRepository _authRepository;
	private readonly IHashService _hashService;
	private readonly IOtpService _otpService;

	public RegisterService(
		IEmailService emailService,
		IPasswordHasherService passwordHasherService,
		IAuthRepository authRepository,
		IHashService hashService,
		IOtpService otpService,
		ILogger<RegisterService> logger)
	{
		this._emailService = emailService;
		this._passwordHasherService = passwordHasherService;
		this._logger = logger;
		this._authRepository = authRepository;
		this._hashService = hashService;
		this._otpService = otpService;
	}

	public async Task<OtpVerificationDTO> RegisterAsync(RegisterRequestDTO registerRequestDTO)
	{

		_logger.LogInformation("Starting registration process for email: {Email}", registerRequestDTO.Email);

		var isUserEmailExist = await _authRepository.IsUserEmailExistAsync(registerRequestDTO.Email);

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

		var otpBody = await _emailService.SendOtpBody(registerRequestDTO.Email, otp);

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


		var userDTO = new OtpVerificationDTO
		 (
		  Email: registerRequestDTO.Email,
		  OtpCodeHash: HashOTP,
		  IsVerified: false,
		  IsUsed: false,
		  AttemptCount: 0,
		  CreatedAt: DateTime.UtcNow,
		  ExpiresAt: DateTime.UtcNow.AddMinutes(10),
		  VerifiedAt: null
		 );

		var otpUser = await _authRepository.InsertOtpVerification(userDTO);

		_logger.LogInformation("Stored OTP verification record for email: {Email}", registerRequestDTO.Email);

		if (otpUser == null)
		{
			_logger.LogError("Failed to store OTP verification record for email: {Email}", registerRequestDTO.Email);
			throw new Exception("Failed to store OTP verification record.");
		}

		return userDTO;
	}
}
