namespace Auth.Services;

public interface IForgotPassword
{
	Task<Guid> ForgotPasswordAsync(string email);

	Task<bool> ResetPasswordAsync(Guid id, string newPassword);

	Task<bool> IsTokenValid(string tokeHash);

}
