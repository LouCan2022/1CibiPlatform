
using Auth.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Services;
public class JWTService : IJWTService
{
    private readonly IConfiguration _configuration;

    public JWTService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetAccessToken
        (LoginDTO loginDTO)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);

        var tokenHandler = new JwtSecurityTokenHandler();
        var symKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(GetClaims(loginDTO)),
            Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(symKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private IEnumerable<Claim> GetClaims(LoginDTO loginDTO)
    {
        return new List<Claim>
        {
            new Claim("userId", loginDTO.Id.ToString()),
            new Claim("username", loginDTO.Username),
            new Claim("email", loginDTO.Email),
            new Claim("FullName", $"{loginDTO.FirstName} {loginDTO.MiddleName} {loginDTO.LastName}"),
            new Claim("appId", string.Join(",", loginDTO.AppId)),
            new Claim("role", string.Join(",",  loginDTO.roleId))
        };
    }
}
