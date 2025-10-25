namespace Auth.Features.GetUserIdForForgotPassword;

public record GetUserIdForForgotPasswordCommand(GetUserIdForForgotPasswordRequest GetUserIdForForgotPasswordRequest) : ICommand<GetUserIdForForgotPasswordResult>;

public record GetUserIdForForgotPasswordResult(Guid UserId);

public class GetUserIdForForgotPasswordCommandValidator : AbstractValidator<GetUserIdForForgotPasswordCommand>
{
	public GetUserIdForForgotPasswordCommandValidator()
	{
		RuleFor(x => x.GetUserIdForForgotPasswordRequest.email)
			.NotEmpty().WithMessage("Email is required.")
			.EmailAddress().WithMessage("Invalid email format.");
	}
}

public class GetUserIdForForgotPasswordHandler : ICommandHandler<GetUserIdForForgotPasswordCommand, GetUserIdForForgotPasswordResult>
{
	private readonly IForgotPassword _forgotPassword;
	public GetUserIdForForgotPasswordHandler(IForgotPassword forgotPassword)
	{
		_forgotPassword = forgotPassword;
	}
	public async Task<GetUserIdForForgotPasswordResult> Handle(GetUserIdForForgotPasswordCommand request, CancellationToken cancellationToken)
	{
		Guid userId = await _forgotPassword.ForgotPasswordAsync(request.GetUserIdForForgotPasswordRequest.email);

		return new GetUserIdForForgotPasswordResult(userId);
	}
}



