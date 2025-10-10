namespace FrontendWebassembly.DTO.Auth;
public record LoginCred(
	string Username,
	string Password,
	bool IsRememberMe);