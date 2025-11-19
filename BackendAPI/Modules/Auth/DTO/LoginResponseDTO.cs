namespace Auth.DTO;
public record LoginResponseDTO
	(string userId,
	 string access_token,
	 string token_type = "bearer",
	 int expires_in = 0,
	 string name = default!,
	 string email = default!,
	 string issued = default!,
	 string expires = default!);