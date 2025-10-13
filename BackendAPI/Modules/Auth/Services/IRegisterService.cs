namespace Auth.Services;

public interface IRegisterService
{
	Task<bool> RegisterAsync(RegisterRequestDTO registerRequestDTO);
}
