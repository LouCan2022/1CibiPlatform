namespace ATS.Services.EmailService;

/// <summary>
/// Reads an SMTP failure the same way wherever it happened - during CONNECT, during
/// AUTHENTICATE, or during SEND.
///
/// This is shared rather than living in the send path because the first version of this
/// code only classified send failures. "454 Too many login attempts" is raised by
/// AuthenticateAsync, so it escaped the send path's try/catch entirely, was reported as an
/// unclassified transient fault, and got retried - which opened another connection and
/// produced another 454. The classifier has to be reachable from the place the connection
/// is built, or the login throttle is invisible to the code that must react to it.
/// </summary>
public static class SmtpFailureClassifier
{
	/// <summary>
	/// True when the message says the provider is deliberately slowing this sender down,
	/// rather than reporting something about the recipient.
	/// </summary>
	private static bool LooksLikeThrottle(string? message) =>
		message is not null
		&& (message.Contains("try again later", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("unusual rate", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("too many", StringComparison.OrdinalIgnoreCase));

	/// <summary>
	/// Classifies a failure raised while CONNECTING or AUTHENTICATING.
	///
	/// Anything throttle-shaped here is a LOGIN throttle, which is a different budget from
	/// the send rate and needs the longer back-off.
	/// </summary>
	public static EmailDeliveryResult ClassifyConnectFailure(Exception exception)
	{
		if (exception is MailKit.Security.AuthenticationException authException)
		{
			// MailKit wraps the server's refusal. A throttle-shaped message is the provider
			// rate limiting logins; anything else is a genuinely bad credential, which no
			// amount of retrying will fix.
			return LooksLikeThrottle(authException.Message)
				? EmailDeliveryResult.Throttled("454", authException.Message)
				: EmailDeliveryResult.Permanent(null, authException.Message);
		}

		if (exception is MailKit.Net.Smtp.SmtpCommandException commandException)
		{
			var code = (int)commandException.StatusCode;

			if (code is 421 or 454 || LooksLikeThrottle(commandException.Message))
			{
				return EmailDeliveryResult.Throttled(
					code.ToString(CultureInfo.InvariantCulture),
					commandException.Message);
			}

			return code >= 500
				? EmailDeliveryResult.Permanent(
					code.ToString(CultureInfo.InvariantCulture),
					commandException.Message)
				: EmailDeliveryResult.Transient(
					code.ToString(CultureInfo.InvariantCulture),
					commandException.Message);
		}

		// A socket that will not open, a TLS negotiation that failed, a DNS miss. Transient:
		// the provider may simply be unreachable right now.
		return EmailDeliveryResult.Transient(null, exception.Message);
	}

	/// <summary>
	/// Classifies a failure raised while SENDING, and says whether the session survives.
	///
	/// The second half matters as much as the first. Discarding a session forces the next
	/// caller to log in again, so treating every failure as fatal to the connection turns
	/// one throttle into a stream of logins - the exact loop that produced "454 Too many
	/// login attempts" here.
	/// </summary>
	public static (EmailDeliveryResult Result, bool SessionIsUsable) ClassifySendFailure(
		MailKit.Net.Smtp.SmtpCommandException exception)
	{
		var code = (int)exception.StatusCode;
		var codeText = code.ToString(CultureInfo.InvariantCulture);

		if (code is 421 or 454 || LooksLikeThrottle(exception.Message))
		{
			// 421 is "service closing transmission channel" - the server has hung up, so the
			// session is genuinely gone. Every other throttle leaves the connection open,
			// and keeping it is what avoids a re-login.
			var serverClosedConnection = code == 421;

			return (
				EmailDeliveryResult.Throttled(codeText, exception.Message),
				!serverClosedConnection);
		}

		// A per-recipient rejection says nothing about the connection: the session is still
		// good and the next message can go down it.
		return code >= 500
			? (EmailDeliveryResult.Permanent(codeText, exception.Message), true)
			: (EmailDeliveryResult.Transient(codeText, exception.Message), true);
	}
}
