namespace BuildingBlocks.SharedServices.Implementations;

public class HashService : IHashService
{
	public string Hash(string input)
	{
		using var sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
		return Convert.ToBase64String(hashBytes)
					  .Replace('+', '-')
					  .Replace('/', '_')
					  .TrimEnd('=');
	}

	public bool Verify(string inputHash, string hash)
	{
		// Convert Base64Url back to standard Base64 before decoding
		string ToBase64(string base64Url)
		{
			string padded = base64Url.Replace('-', '+').Replace('_', '/');
			switch (padded.Length % 4)
			{
				case 2: padded += "=="; break;
				case 3: padded += "="; break;
			}
			return padded;
		}

		return CryptographicOperations.FixedTimeEquals(
			Convert.FromBase64String(ToBase64(inputHash)),
			Convert.FromBase64String(ToBase64(hash))
		);
	}
}
