namespace Auth.Features.GetNewAccessToken;

public record GetNewAccessTokenCommand(Guid userId, string refreshToken) : ICommand<GetNewAccessTokenResult>;

public record GetNewAccessTokenResult(LoginResponseWebDTO loginResponseWebDTO);

public class GetNewAccessTokenCommandValidator : AbstractValidator<GetNewAccessTokenCommand>
{
	public GetNewAccessTokenCommandValidator()
	{
		RuleFor(x => x.userId)
			.NotEmpty().WithMessage("User ID is required.");

		RuleFor(x => x.refreshToken)
			.NotEmpty().WithMessage("Refresh token is required.");
	}
}

public class GetNewAccessTokenHandler : ICommandHandler<GetNewAccessTokenCommand, GetNewAccessTokenResult>
{
	private readonly IRefreshTokenService _refreshTokenService;

	public GetNewAccessTokenHandler(IRefreshTokenService refreshTokenService)
	{
		this._refreshTokenService = refreshTokenService;
	}

	public async Task<GetNewAccessTokenResult> Handle(GetNewAccessTokenCommand request, CancellationToken cancellationToken)
	{
		var newSetOfTokens = await this._refreshTokenService
			.GetNewAccessTokenAsync(request.userId, request.refreshToken);

		return new GetNewAccessTokenResult(newSetOfTokens);
	}
}
