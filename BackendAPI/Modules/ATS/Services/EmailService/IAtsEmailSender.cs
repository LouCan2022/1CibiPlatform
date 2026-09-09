namespace ATS.Services.EmailService;

/// <summary>
/// The result-aware send, kept separate from the shared <c>IEmailService</c>.
///
/// <c>IEmailService</c> lives in BuildingBlocks and is implemented by Auth and the test
/// fakes as well; widening it would force every implementer to reason about SMTP status
/// codes they do not have. Only the ATS bulk path needs to tell "slow down" apart from
/// "no such mailbox", so only ATS declares that contract.
/// </summary>
public interface IAtsEmailSender
{
	Task<EmailDeliveryResult> SendATSEmailWithResultAsync(
		string toEmail,
		string subject,
		string body,
		CancellationToken cancellationToken);
}
