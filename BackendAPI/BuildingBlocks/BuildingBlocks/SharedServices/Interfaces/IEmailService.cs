namespace BuildingBlocks.SharedServices.Interfaces;

public interface IEmailService
{
	Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

	Task<string> SendOtpBody(string toEmai, string name, string otpCode);

	Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink);
}
