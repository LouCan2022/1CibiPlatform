namespace Auth.Features.Login;

public record LoginCommand(LoginCred LoginCred) : ICommand<LoginResult>;

public record LoginResult(LoginResponseDTO loginResponseDTO);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginCred.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters.");
        RuleFor(x => x.LoginCred.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginHandler : ICommandHandler<LoginCommand, LoginResult>
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IConfiguration _configuration;
    private readonly IJWTService _jWTService;

    public LoginHandler(
        IAuthRepository authRepository,
        IPasswordHasherService passwordHasherService,
        IConfiguration configuration,
        IJWTService jWTService)
    {
        this._authRepository = authRepository;
        this._passwordHasherService = passwordHasherService;
        this._configuration = configuration;
        this._jWTService = jWTService;
    }

    public async Task<LoginResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // mapping LoginCommand to LoginCred
        LoginCred cred = request.LoginCred.Adapt<LoginCred>();

        // fetching user data from database
        LoginDTO userData = await this._authRepository.LoginAsync(cred);

        // checking if client credentials are valid
        if (userData.Id == Guid.Empty)
        {
            throw new NotFoundException("Invalid username or password.");
        }

        // verifying password
        bool isPasswordValid = this._passwordHasherService.VerifyPassword(userData.PasswordHash, request.LoginCred.Password);

        if (!isPasswordValid)
        {
            throw new NotFoundException("Invalid username or password.");
        }

        // produce JWT token
        string jwtToken = this._jWTService.GetAccessToken(userData);

        double configTime = double.Parse(_configuration.GetSection("Jwt:ExpiryInMinutes").Value!);

        var expireIn = (int)(configTime * 60);

        var response = new LoginResponseDTO(
            userData.Id.ToString()!,
            jwtToken,
            "bearer",
            expireIn,
            "admin",
            DateTime.Now.ToString(),
            DateTime.Now.AddMinutes(configTime).ToString()
        );

        return new LoginResult(response);
    }
}



