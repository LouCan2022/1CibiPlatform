
namespace Auth.Services;
public class JWTService : IJWTService
{
    private readonly IConfiguration _configuration;

    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    //public string GetAccessToken
    //    (ClientLoginDataDTO clientLoginDataDTO)
    //{
    //    var jwtSettings = _configuration.GetSection("Jwt");
    //    var key = jwtSettings["Key"];
    //    var issuer = jwtSettings["Issuer"];
    //    var audience = jwtSettings["Audience"];
    //    var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);

    //    var tokenHandler = new JwtSecurityTokenHandler();
    //    var symKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));

    //    var tokenDescriptor = new SecurityTokenDescriptor
    //    {
    //        Subject = new ClaimsIdentity(GetClaims(clientLoginDataDTO)),
    //        Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
    //        Issuer = issuer,
    //        Audience = audience,
    //        SigningCredentials = new SigningCredentials(symKey, SecurityAlgorithms.HmacSha256Signature)
    //    };

    //    var token = tokenHandler.CreateToken(tokenDescriptor);

    //    return tokenHandler.WriteToken(token);
    //}

    //private IEnumerable<Claim> GetClaims(ClientLoginDataDTO clientLoginDataDTO)
    //{
    //    return new List<Claim>
    //    {
    //        new Claim("username", clientLoginDataDTO.Username),
    //        new Claim("blueusername" , clientLoginDataDTO.BlueUsername),
    //        new Claim("bluetoken", clientLoginDataDTO.BlueToken),
    //        // Add other claims as needed
    //    };
    //}
}
