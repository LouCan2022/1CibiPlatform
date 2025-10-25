namespace BuildingBlocks.SharedServices.Interfaces;

public interface IEmailService
{
	Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

	string SendOtpBody(string name, string otpCode);

	string SendPasswordResetBody(string name, string resetLink);
}
