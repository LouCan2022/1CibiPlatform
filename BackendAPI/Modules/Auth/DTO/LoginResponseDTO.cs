namespace Auth.DTO;
public record LoginResponseDTO
    (string userId,
     string Access_token,
     string Token_type = "bearer",
     int expires_in = 0,
     string UserName = default!,
     string Issued = default!,
     string Expires = default!);