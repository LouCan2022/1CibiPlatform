namespace ATS.Features.Web.GetApplicationFormPreview;

public record GetApplicationFormPreviewQueryRequest(Guid EmailInvitationRequestId) : IQuery<GetApplicationFormPreviewQueryResult>;

public record GetApplicationFormPreviewQueryResult(ApplicationFormPreviewDTO Preview);

public class GetApplicationFormPreviewQueryRequestValidator : AbstractValidator<GetApplicationFormPreviewQueryRequest>
{
	public GetApplicationFormPreviewQueryRequestValidator()
	{
		RuleFor(x => x.EmailInvitationRequestId)
			.NotEmpty()
			.WithMessage("Email invitation request ID is required.");
	}
}

public class GetApplicationFormPreviewHandler : IQueryHandler<GetApplicationFormPreviewQueryRequest, GetApplicationFormPreviewQueryResult>
{
	private readonly IReportService _reportService;

	public GetApplicationFormPreviewHandler(IReportService reportService)
	{
		_reportService = reportService;
	}

	public async Task<GetApplicationFormPreviewQueryResult> Handle(GetApplicationFormPreviewQueryRequest request, CancellationToken cancellationToken)
	{
		var preview = await _reportService.GetApplicationFormPreviewAsync(request.EmailInvitationRequestId, cancellationToken);
		return new GetApplicationFormPreviewQueryResult(preview);
	}
}
