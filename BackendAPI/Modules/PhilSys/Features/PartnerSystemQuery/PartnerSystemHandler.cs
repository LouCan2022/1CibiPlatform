﻿namespace PhilSys.Features.PartnerSystemQuery;
public record PartnerSystemCommand(string callback_url, string inquiry_type, IdentityData identity_data) : ICommand<PartnerSystemResult>;
public record PartnerSystemResult(PartnerSystemResponseDTO PartnerSystemResponseDTO);
public class PartnerSystemCommandValidator : AbstractValidator<PartnerSystemCommand>
{
	public PartnerSystemCommandValidator()
	{
		// Always required fields
		RuleFor(x => x.callback_url)
			.NotEmpty().WithMessage("callback_url is required.");

		RuleFor(x => x.inquiry_type)
			.NotEmpty().WithMessage("inquiry_type is required.")
			.Must(t => t == "name_dob" || t == "pcn")
			.WithMessage("inquiry_type must be either 'name_dob' or 'pcn'.");

		// When InquiryType = "name_dob"
		When(x => x.inquiry_type == "name_dob", () =>
		{
			RuleFor(x => x.identity_data.FirstName)
				.NotEmpty().WithMessage("First Name is required for 'name_dob' inquiry.")
				.MaximumLength(50).WithMessage("First Name must not exceed 50 characters.");

			RuleFor(x => x.identity_data.MiddleName)
				.NotEmpty().WithMessage("Middle Name is required for 'name_dob' inquiry.")
				.MaximumLength(50).WithMessage("Middle Name must not exceed 50 characters.");

			RuleFor(x => x.identity_data.LastName)
				.NotEmpty().WithMessage("Last Name is required for 'name_dob' inquiry.")
				.MaximumLength(50).WithMessage("Last Name must not exceed 50 characters.");

			RuleFor(x => x.identity_data.BirthDate)
				.NotEmpty().WithMessage("Birth Date is required for 'name_dob' inquiry.")
				.Matches(@"^\d{4}-\d{2}-\d{2}$")
				.WithMessage("Birth Date must be in format YYYY-MM-DD.");
		});

		// When InquiryType = "pcn"
		When(x => x.inquiry_type == "pcn", () =>
		{
			RuleFor(x => x.identity_data.PCN)
				.NotEmpty().WithMessage("PCN is required for 'pcn' inquiry.")
				.Matches(@"^\d{16}$")
				.WithMessage("PCN must be exactly 16 digits.");
		});
	}
}
public class PartnerSystemHandler : ICommandHandler<PartnerSystemCommand, PartnerSystemResult>
{
	private readonly PartnerSystemService _partnerSystemService;
	public PartnerSystemHandler(PartnerSystemService PartnerSystemService)
	{
		_partnerSystemService = PartnerSystemService;
	}
	public async Task<PartnerSystemResult> Handle(PartnerSystemCommand request, CancellationToken cancellationToken)
	{
		var result = await _partnerSystemService.PartnerSystemQueryAsync(request.callback_url, request.inquiry_type, request.identity_data);
		return new PartnerSystemResult(result);
	}
}
