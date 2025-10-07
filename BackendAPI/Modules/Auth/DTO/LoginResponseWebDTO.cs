namespace Auth.DTO;

public record LoginResponseWebDTO
	(string userId,
	 string Access_token,
	 string refresh_token,
	 string Token_type = "bearer",
	 int expires_in = 0,
	 string UserName = default!,
	 List<int> Appid = default!,
	 List<List<int>> SubMenuid = default!,
	 List<int> RoleId = default!,
	 string Issued = default!,
	 string Expires = default!);