namespace PhilSys.Features.PartnerSystemQuery;

public record PartnerSystemCommand(PartnerSystemRequestDTO PartnerSystemRequestDTO) : ICommand<PartnerSystemResult>;

public record PartnerSystemResult(PartnerSystemResponseDTO PartnerSystemResponseDTO);


public class PartnerSystemCommandValidator : AbstractValidator<PartnerSystemCommand>
{
	public PartnerSystemCommandValidator()
	{
		RuleFor(x => x.PartnerSystemRequestDTO).NotNull();

		// Always required fields
		RuleFor(x => x.PartnerSystemRequestDTO.ExternalUserId)
			.NotEmpty().WithMessage("External User ID is required.")
			.MaximumLength(100).WithMessage("external_user_id must not exceed 100 characters.");

		RuleFor(x => x.PartnerSystemRequestDTO.CallbackUrl)
			.NotEmpty().WithMessage("callback_url is required.")
			.Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
			.WithMessage("callback_url must be a valid URL.");

		RuleFor(x => x.PartnerSystemRequestDTO.InquiryType)
			.NotEmpty().WithMessage("inquiry_type is required.")
			.Must(t => t == "name_dob" || t == "pcn")
			.WithMessage("inquiry_type must be either 'name_dob' or 'pcn'.");

		RuleFor(x => x.PartnerSystemRequestDTO.IdentityData)
			.NotNull().WithMessage("identity_data is required.");

		// When InquiryType = "name_dob"
		When(x => x.PartnerSystemRequestDTO.InquiryType == "name_dob", () =>
		{
			RuleFor(x => x.PartnerSystemRequestDTO.IdentityData.FirstName)
				.NotEmpty().WithMessage("First Name is required for 'name_dob' inquiry.")
				.MaximumLength(50).WithMessage("First Name must not exceed 50 characters.");

			RuleFor(x => x.PartnerSystemRequestDTO.IdentityData.MiddleName)
				.NotEmpty().WithMessage("Middle Name is required for 'name_dob' inquiry.")
				.MaximumLength(50).WithMessage("Middle Name must not exceed 50 characters.");

			RuleFor(x => x.PartnerSystemRequestDTO.IdentityData.LastName)
				.NotEmpty().WithMessage("Last Name is required for 'name_dob' inquiry.")
				.MaximumLength(50).WithMessage("Last Name must not exceed 50 characters.");

			RuleFor(x => x.PartnerSystemRequestDTO.IdentityData.BirthDate)
				.NotEmpty().WithMessage("Birth Date is required for 'name_dob' inquiry.")
				.Matches(@"^\d{4}-\d{2}-\d{2}$")
				.WithMessage("Birth Date must be in format YYYY-MM-DD.");
		});

		// When InquiryType = "pcn"
		When(x => x.PartnerSystemRequestDTO.InquiryType == "pcn", () =>
		{
			RuleFor(x => x.PartnerSystemRequestDTO.IdentityData.PCN)
				.NotEmpty().WithMessage("PCN is required for 'pcn' inquiry.")
				.Matches(@"^\d{16}$")
				.WithMessage("PCN must be exactly 16 digits.");
		});
	}
}
public class PartnerSystemHandler : ICommandHandler<PartnerSystemCommand, PartnerSystemResult>
{
	private readonly PartnerSystemService _partnerSystemService;
	private readonly ILogger<PartnerSystemHandler> _logger;

	public PartnerSystemHandler(PartnerSystemService PartnerSystemService, ILogger<PartnerSystemHandler> logger)
	{
		_partnerSystemService = PartnerSystemService;
		_logger = logger;
	}
	public async Task<PartnerSystemResult> Handle(PartnerSystemCommand request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Handling Philsys partner system query request for client: {ClientName}", request.PartnerSystemRequestDTO.ExternalUserId);

		var result = await _partnerSystemService.PartnerSystemQueryAsync(request.PartnerSystemRequestDTO);

		_logger.LogInformation("Successfully retrieved the Response");

		return new PartnerSystemResult(result);
	}
}
