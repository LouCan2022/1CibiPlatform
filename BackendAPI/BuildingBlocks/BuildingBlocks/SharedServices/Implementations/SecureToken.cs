namespace BuildingBlocks.SharedServices.Implementations;

public class SecureToken : ISecureToken
{
	public string GenerateSecureToken()
	{
		byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
		return Convert.ToBase64String(randomBytes)
			.Replace('+', '-')
			.Replace('/', '_')
			.Replace("=", "");
	}
}
