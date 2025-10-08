namespace Auth.DTO;

public record LoginDTO(
	Guid Id,
	string Username,
	string PasswordHash,
	string Email,
	string FirstName,
	string LastName,
	string? MiddleName,
	List<int> AppId,
	List<List<int>> SubMenuId,
	List<int> roleId
	);

public record UserDataDTO(
	Guid Id,
	string Username,
	string PasswordHash,
	string Email,
	string FirstName,
	string LastName,
	string? MiddleName,
	string refreshToken,
	List<int> AppId,
	List<List<int>> SubMenuId,
	List<int> roleId
	);



public record LoginCred(
	string Username,
	string Password);


public record LoginWebCred(
	string Username,
	string Password,
	bool IsRememberMe);