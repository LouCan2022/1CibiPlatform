namespace Auth.Services;

public interface IForgotPasswordService
{
	Task<Guid> ForgotPasswordAsync(string email);

	Task<bool> ResetPasswordAsync(Guid id, string hashToken, string newPassword);

	Task<bool> IsTokenValid(Guid userId, string tokenHash);

}
