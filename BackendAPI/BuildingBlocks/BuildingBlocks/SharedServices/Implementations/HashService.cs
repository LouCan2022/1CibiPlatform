using BuildingBlocks.SharedServices.Interfaces;
using System.Security.Cryptography;

namespace BuildingBlocks.SharedServices.Implementations;

public class HashService : IHashService
{
	public string Hash(string input)
	{
		using var sha256 = SHA256.Create();
		var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
		return Convert.ToBase64String(hashBytes);
	}

	public bool Verify(string inputHash, string hash)
	{
		return CryptographicOperations.FixedTimeEquals(
			Convert.FromBase64String(inputHash),
			Convert.FromBase64String(hash)
		);
	}
}
