namespace ATS.Services.EmailService;

/// <summary>
/// What the SMTP server actually said, rather than the bool the send used to return.
///
/// The distinction is the whole point: a mistyped address and a "you are going too fast"
/// are both failures, but retrying the first wastes the row's budget and retrying the
/// second is what extends a ten-minute throttle into an hour.
/// </summary>
public enum EmailDeliveryOutcome
{
	/// <summary>The server accepted the message. It owns delivery from here.</summary>
	Sent,

	/// <summary>
	/// A temporary fault (4xx, socket drop, timeout). Worth retrying after a back-off.
	/// </summary>
	Transient,

	/// <summary>
	/// The server refused permanently (5xx): unknown mailbox, rejected sender, malformed
	/// address. Retrying produces the identical refusal, so the row fails immediately
	/// instead of consuming five attempts.
	/// </summary>
	Permanent,

	/// <summary>
	/// The provider is rate limiting this sender (421, 454, "try again later"). Every
	/// remaining send in the pass must stop - not slow down, stop. Continuing to knock is
	/// what turns a short deferral into a long block.
	/// </summary>
	Throttled
}

public sealed record EmailDeliveryResult(
	EmailDeliveryOutcome Outcome,
	string? StatusCode = null,
	string? Message = null)
{
	public bool IsSent => Outcome == EmailDeliveryOutcome.Sent;

	public static readonly EmailDeliveryResult Sent = new(EmailDeliveryOutcome.Sent);

	public static EmailDeliveryResult Transient(string? code, string? message) =>
		new(EmailDeliveryOutcome.Transient, code, message);

	public static EmailDeliveryResult Permanent(string? code, string? message) =>
		new(EmailDeliveryOutcome.Permanent, code, message);

	public static EmailDeliveryResult Throttled(string? code, string? message) =>
		new(EmailDeliveryOutcome.Throttled, code, message);
}
