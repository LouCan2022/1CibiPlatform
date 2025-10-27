using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace BuildingBlocks.SharedServices.Implementations;

public class EmailService : IEmailService
{

	private readonly IConfiguration _configuration;
	private readonly ILogger<EmailService> _logger;
	private readonly string _senderEmail;
	private readonly string _appPassword;
	private readonly string _smtpHost;
	private readonly int _smtpPort;
	private readonly int _expirationInMinutes;

	public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
	{
		_configuration = configuration;
		_logger = logger;

		// Load from appsettings.json
		_senderEmail = _configuration["Email:Gmail:SenderEmail"]
			?? throw new InvalidOperationException("Email:Gmail:SenderEmail not configured");
		_appPassword = _configuration["Email:Gmail:AppPassword"]
			?? throw new InvalidOperationException("Email:Gmail:AppPassword not configured");
		_smtpHost = _configuration["Email:Gmail:SmtpHost"] ?? "smtp.gmail.com";
		_smtpPort = int.Parse(_configuration["Email:Gmail:SmtpPort"] ?? "587");
		_expirationInMinutes = int.Parse(_configuration["Email:OtpExpirationInMinutes"] ?? "15");
	}

	public async Task<bool> SendEmailAsync(
		string toEmail,
		string subject,
		string body,
		bool isHtml = true)
	{
		try
		{
			using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
			{
				// Gmail requires TLS
				smtpClient.EnableSsl = true;
				smtpClient.UseDefaultCredentials = false;
				smtpClient.Credentials = new NetworkCredential(_senderEmail, _appPassword);
				smtpClient.Timeout = 10000; // 10 seconds timeout

				using (var mailMessage = new MailMessage())
				{
					mailMessage.From = new MailAddress(_senderEmail, "NoSent Auth");
					mailMessage.To.Add(toEmail);
					mailMessage.Subject = subject;
					mailMessage.Body = body;
					mailMessage.IsBodyHtml = isHtml;

					await smtpClient.SendMailAsync(mailMessage);

					_logger.LogInformation($"Email sent successfully to {toEmail}");
					return true;
				}
			}
		}
		catch (SmtpException ex)
		{
			_logger.LogError($"SMTP Error sending email to {toEmail}: {ex.Message}");
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
			return false;
		}
	}


	public string SendOtpBody(
		string name,
		string otpCode)
	{
		string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .otp-box {{ 
                            background-color: #fff; 
                            border: 2px solid #007bff; 
                            padding: 20px; 
                            text-align: center; 
                            font-size: 32px; 
                            font-weight: bold; 
                            letter-spacing: 5px;
                            margin: 20px 0;
                        }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Email Verification</h1>
                        </div>
                        <div class='content'>
                            <p>Hello {name},</p>
                            <p>Thank you for registering with us. Please use the following code to verify your email address:</p>
                            <div class='otp-box'>{otpCode}</div>
                            <p>This code will expire in {_expirationInMinutes} minutes.</p>
                            <p>If you did not create this account, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 NoSent. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

		return body;
	}

	/// <summary>
	/// Send password reset email
	/// </summary>
	public string SendPasswordResetBody(
		string name,
		string resetLink,
		int expireMins)
	{
		string subject = "Password Reset Request";
		string body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ 
                            display: inline-block;
                            background-color: #dc3545;
                            color: white;
                            padding: 12px 30px;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 20px 0;
                        }}
                        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Password Reset</h1>
                        </div>
                        <div class='content'>
                            <p>Hello {name},</p>
                            <p>We received a request to reset your password. Click the button below to reset it:</p>
                            <a href='{resetLink}' class='button'>Reset Password</a>
                            <p>This link will expire in {expireMins} minutes.</p>
                            <p>If you did not request this, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 NoSent. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

		return body;
	}
}

