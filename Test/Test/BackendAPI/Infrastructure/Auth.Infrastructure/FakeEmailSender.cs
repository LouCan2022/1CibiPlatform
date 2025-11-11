using BuildingBlocks.SharedServices.Interfaces;

namespace Test.BackendAPI.Infrastructure.Auth.Infrastructure;

public class FakeEmailSender : IEmailService
{
	public Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
		=> Task.FromResult(true);

	public string SendOtpBody(string name, string otpCode)
		=> $"Hello {name}, your OTP code is {otpCode}";

	public string SendPasswordResetBody(string name, string resetLink, int expireMins)
		=> $"Hello {name}, to reset your password, please use the following link: {resetLink}. This link will expire in {expireMins} minutes.";
}
