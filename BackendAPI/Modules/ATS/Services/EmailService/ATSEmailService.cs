namespace ATS.Services.EmailService;

public class ATSEmailService : IEmailService, IAtsEmailSender
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<ATSEmailService> _logger;
	private readonly SmtpConnectionPool _connectionPool;
	private readonly SmtpRateLimiter _rateLimiter;
	private readonly AtsEmailDeliveryOptions _options;
	private readonly int _atsApplicationFormExpirationInHours;

	public ATSEmailService(
		IConfiguration configuration,
		ILogger<ATSEmailService> logger,
		SmtpConnectionPool connectionPool,
		SmtpRateLimiter rateLimiter,
		IOptions<AtsEmailDeliveryOptions> options)
	{
		_configuration = configuration;
		_logger = logger;
		_connectionPool = connectionPool;
		_rateLimiter = rateLimiter;
		_options = options.Value;
		_atsApplicationFormExpirationInHours = _configuration.GetSection("ATS").GetValue<int>("ATSApplicationFormExpiryInHours");
	}

	/// <summary>
	/// Kept for callers that only need "did it go out" - the dispute mail and the single
	/// enrolment path. The bulk processor uses <see cref="SendATSEmailWithResultAsync"/>
	/// instead, because it has to tell a rate limit apart from a bad address.
	/// </summary>
	public async Task<bool> SendATSEmailAsync(string toEmail, string subject, string body)
	{
		var result = await SendATSEmailWithResultAsync(toEmail, subject, body, CancellationToken.None);

		return result.IsSent;
	}

	/// <summary>
	/// Sends one message over a pooled, already-authenticated session, pacing it through the
	/// process-wide rate limiter first.
	///
	/// Both of those exist because of a real incident: a per-message SmtpClient meant one
	/// AUTH LOGIN per email, and Gmail stopped this sender after 14 messages in about eight
	/// seconds. The session is now reused and the send rate is bounded, so the traffic looks
	/// like a mail client rather than a login flood.
	/// </summary>
	public async Task<EmailDeliveryResult> SendATSEmailWithResultAsync(
		string toEmail,
		string subject,
		string body,
		CancellationToken cancellationToken)
	{
		// Paced before the connection is leased. Waiting while holding a session would idle
		// a scarce resource for no reason.
		await _rateLimiter.WaitForSlotAsync(cancellationToken);

		await using var lease = await _connectionPool.AcquireAsync(cancellationToken);

		var message = BuildMessage(toEmail, subject, body);

		try
		{
			await lease.Client.SendAsync(message, cancellationToken);

			lease.RecordSend();

			_logger.LogInformation("Email sent successfully to {Email}", toEmail);

			return EmailDeliveryResult.Sent;
		}
		catch (MailKit.Net.Smtp.SmtpCommandException exception)
		{
			// The server answered with a status code. This is the branch the old
			// implementation threw away by collapsing everything to `false`.
			return ClassifyCommandFailure(toEmail, exception, lease);
		}
		catch (MailKit.Net.Smtp.SmtpProtocolException exception)
		{
			// The conversation itself broke down. The session is not trustworthy.
			lease.MarkFaulted();

			_logger.LogError(
				exception,
				"SMTP protocol error sending to {Email}. The session was discarded.",
				toEmail);

			return EmailDeliveryResult.Transient(null, exception.Message);
		}
		catch (Exception exception) when (exception is IOException or SocketException or TimeoutException or OperationCanceledException
			&& !cancellationToken.IsCancellationRequested)
		{
			// Socket dropped or timed out. Transient, but the connection is dead.
			//
			// Note this can fire AFTER the provider accepted the message - which is exactly
			// how a candidate received the same invitation more than once. The generous
			// SendTimeoutSeconds default exists to make this rare rather than routine.
			lease.MarkFaulted();

			_logger.LogWarning(
				exception,
				"SMTP transport failure sending to {Email}. Treating as transient.",
				toEmail);

			return EmailDeliveryResult.Transient(null, exception.Message);
		}
	}

	private MimeKit.MimeMessage BuildMessage(string toEmail, string subject, string body)
	{
		var message = new MimeKit.MimeMessage();

		message.From.Add(new MimeKit.MailboxAddress("Workforce Manager", _connectionPool.SenderEmail));
		message.To.Add(MimeKit.MailboxAddress.Parse(toEmail));
		message.Subject = subject;

		message.Body = new MimeKit.BodyBuilder
		{
			HtmlBody = body
		}.ToMessageBody();

		return message;
	}

	/// <summary>
	/// Turns an SMTP status code into a retry decision.
	///
	/// The three outcomes lead to genuinely different behaviour upstream, which is why this
	/// is not a bool: a permanent refusal fails the row immediately instead of burning five
	/// attempts, and a throttle stops the entire pass rather than retrying into a wall.
	/// </summary>
	private EmailDeliveryResult ClassifyCommandFailure(
		string toEmail,
		MailKit.Net.Smtp.SmtpCommandException exception,
		SmtpLease lease)
	{
		var code = (int)exception.StatusCode;
		var codeText = code.ToString(CultureInfo.InvariantCulture);

		// 421 (service closing) and 454 (too many auth attempts) are the two Gmail returns
		// when it is deliberately slowing a sender down. 421 also closes the connection, so
		// the session must not go back in the pool.
		var isThrottle =
			code is 421 or 454
			|| exception.Message.Contains("try again later", StringComparison.OrdinalIgnoreCase)
			|| exception.Message.Contains("unusual rate", StringComparison.OrdinalIgnoreCase)
			|| exception.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase);

		if (isThrottle)
		{
			lease.MarkFaulted();

			_logger.LogWarning(
				"SMTP throttling detected while sending to {Email}: {StatusCode} {Message}",
				toEmail,
				codeText,
				exception.Message);

			return EmailDeliveryResult.Throttled(codeText, exception.Message);
		}

		// 4xx is temporary by RFC 5321; 5xx is permanent and will refuse identically on
		// every retry.
		if (code >= 400 && code < 500)
		{
			_logger.LogWarning(
				"Transient SMTP failure sending to {Email}: {StatusCode} {Message}",
				toEmail,
				codeText,
				exception.Message);

			return EmailDeliveryResult.Transient(codeText, exception.Message);
		}

		_logger.LogError(
			"Permanent SMTP rejection for {Email}: {StatusCode} {Message}",
			toEmail,
			codeText,
			exception.Message);

		return EmailDeliveryResult.Permanent(codeText, exception.Message);
	}

	public string SendAppplicationFormNotification(string gmail, string name, string applicationFormLink, string? requestor, string? clientName)
	{
		// Older rows may predate the requestor/client columns, so the sentence
		// degrades to a generic phrasing rather than rendering an empty name.
		var requestorPhrase = string.IsNullOrWhiteSpace(requestor)
			? "The talent acquisition team"
			: WebUtility.HtmlEncode(requestor.Trim());
		var clientPhrase = string.IsNullOrWhiteSpace(clientName)
			? "their company"
			: $"{WebUtility.HtmlEncode(clientName.Trim())} company";

		string body = $@"
			<!DOCTYPE html>
			<html>
			<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial, sans-serif'>
				<div style='max-width:600px;margin:24px auto;background:#ffffff;border:1px solid #d9e5f5;border-radius:12px;overflow:hidden'>
					<div style='padding:24px 36px;background:linear-gradient(100deg, #0b1b3d 0%, #1c3a70 35%, #1d5fd1 75%, #4f93ea 100%);color:#ffffff;text-align:center'>
						<h1 style='margin:0;font-size:20px'>CIBI | Background Verification Information Request</h1>
						<p style='margin:8px 0 0;font-size:13px;line-height:1.5;color:#dbe7fb'>Pre-employment background check — please complete your application form within {_atsApplicationFormExpirationInHours} hours</p>
					</div>
					<div style='padding:34px 36px'>
						<p style='font-size:16px;line-height:1.7'>Dear {name},</p>
						<p style='font-size:16px;line-height:1.7'>
							{requestorPhrase}, talent acquisition {clientPhrase} has requested CIBI Information Inc. to perform background checks on you as part of their pre-employment screening process. Please sign up by clicking the button below:
						</p>
						<p style='margin:28px 0;text-align:center'><a href='{applicationFormLink}' style='display:inline-block;padding:14px 26px;border-radius:999px;background:linear-gradient(100deg, #0b1b3d 0%, #1c3a70 35%, #1d5fd1 75%, #4f93ea 100%);color:#ffffff;text-decoration:none;font-weight:bold'>Application Form</a></p>
						<p style='font-size:15px;line-height:1.6'>Please comply <strong>within the next {_atsApplicationFormExpirationInHours} hours upon receipt of this email</strong> so we can move forward with the completion of verification.</p>
						<p style='font-size:15px;line-height:1.6'><strong>REMINDERS IN ANSWERING THE FORM</strong></p>
						<ol style='font-size:15px;line-height:1.7;margin:0 0 16px;padding-left:20px'>
							<li>In case you do not have a SSS or TIN Number, kindly input random digits from 0 to 9 to proceed with the application.</li>
							<li>In case you have a portion to input the Email Address of HR POC, kindly input your HR person of contact on the company you are applying to.</li>
						</ol>
						<p style='font-size:15px;line-height:1.6'>
							For any questions or concerns, please do not hesitate to reach out to
							<a href='mailto:pre-workteam@cibi.com.ph' style='color:#1d5fd1'>pre-workteam@cibi.com.ph</a>
							and
							<a href='mailto:ceteam@cibi.com.ph' style='color:#1d5fd1'>ceteam@cibi.com.ph</a>
							or call us at +63 923 087 8757 (Sun), or +63 917 632 0486 (Globe).
						</p>
					</div>
					<div style='padding:20px 36px;background:#f4f8fd;color:#66788f;font-size:12px;line-height:1.6'>This e-mail and its attachments may contain sensitive and confidential information. Do not resend, copy, or use this email if you are not the intended recipient. Please contact the sender immediately and delete this entire email. The privilege is not waived because it was delivered to you mistakenly. CIBI Information Inc. and its affiliates accept no liability for any loss or harm resulting from this e-mail and reserve the right to monitor, retain, and/or review email. The opinions stated in this email are solely those of the author and may not reflect the views of CIBI Information Inc. or its affiliates.</div>
				</div>
			</body>
			</html>";

		return body;
	}

	public string SendEmailForDispute(string gmail, string company, string disputeReason, DateTime? orderedAt, string requestor, string subjectName)
	{
		string body = $@"
			<!DOCTYPE html>
			<html>
			<body style='margin:0;padding:0;background:#f4f6fb;font-family:Arial, sans-serif'>
				<div style='max-width:600px;margin:24px auto;background:#ffffff;border:1px solid #d9e5f5;border-radius:12px;overflow:hidden'>
					<div style='padding:24px 36px;background:linear-gradient(100deg, #0b1b3d 0%, #1c3a70 35%, #1d5fd1 75%, #4f93ea 100%);color:#ffffff;text-align:center'>
						<h1 style='margin:0;font-size:20px'>CIBI | Dispute Order Notification</h1>
						<p style='margin:8px 0 0;font-size:13px;line-height:1.5;color:#dbe7fb'>A dispute has been raised on a background check order and requires your review</p>
					</div>
					<div style='padding:34px 36px'>
						<p style='font-size:16px;line-height:1.7'>Hello,</p>
						<p style='font-size:16px;line-height:1.7'>
							A request for dispute has been raised for subject
							<strong>{subjectName}</strong>.
						</p>
						<p style='font-size:15px;line-height:1.6'>Supplemental details are provided below:</p>
						<table role='presentation' style='width:100%;border-collapse:collapse;margin:24px 0;background:#f4f8fd;border:1px solid #d9e5f5;border-radius:12px'>
							<tr><td style='padding:12px 16px;color:#5b6f8f;font-size:13px'>Requestor Email:</td><td style='padding:12px 16px;font-weight:bold'>{requestor}</td></tr>
							<tr><td style='padding:12px 16px;color:#5b6f8f;font-size:13px'>Company:</td><td style='padding:12px 16px;font-weight:bold'>{company}</td></tr>
							<tr><td style='padding:12px 16px;color:#5b6f8f;font-size:13px'>Order Date:</td><td style='padding:12px 16px;font-weight:bold'>{orderedAt}</td></tr>
							<tr><td style='padding:12px 16px;color:#5b6f8f;font-size:13px'>Reason for Dispute:</td><td style='padding:12px 16px;font-weight:bold'>{disputeReason}</td></tr>
						</table>
						<p style='font-size:15px;line-height:1.6'>
							Please review the dispute request and proceed with the appropriate action.
						</p>
						<p style='font-size:15px;line-height:1.6'>Thank you.</p>
					</div>
					<div style='padding:20px 36px;background:#f4f8fd;color:#66788f;font-size:12px;line-height:1.6;text-align:center'>This is an automated notification from the ATS. Please do not reply to this email.</div>
				</div>
			</body>
			</html>";

		return body;
	}

	public string SendApprovalNotificationBody(string gmail)
	{
		throw new NotImplementedException();
	}

	public string SendNotificationBody(string gmail, string application, string submenu, string role)
	{
		throw new NotImplementedException();
	}

	public string SendOtpBody(string name, string otpCode)
	{
		throw new NotImplementedException();
	}

	public string SendPasswordResetBody(string name, string resetLink, int expireMins)
	{
		throw new NotImplementedException();
	}

	public Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
	{
		return SendATSEmailAsync(toEmail, subject, body);
	}
}
