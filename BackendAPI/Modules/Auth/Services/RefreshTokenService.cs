namespace Auth.Services
{
	public class RefreshTokenService : IRefreshTokenService
	{
		public (string, string) GenerateRefreshToken()
		{
			// Generate random token
			var randomNumber = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomNumber);
			var token = Convert.ToBase64String(randomNumber);

			// Hash for storage
			var hashedToken = HashToken(token);

			return (token, hashedToken);
		}


		private string HashToken(string token)
		{
			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
			return Convert.ToBase64String(hashBytes);
		}

		public bool ValidateHashToken(string providedToken, string storedHash)
		{
			var providedHash = HashToken(providedToken);
			return CryptographicOperations.FixedTimeEquals(
				Convert.FromBase64String(providedHash),
				Convert.FromBase64String(storedHash)
			);
		}

		public Task<bool> SaveRefreshToken()
		{
			throw new NotImplementedException();
		}

		public Task<string> ValidateRefreshToken()
		{
			throw new NotImplementedException();
		}


	}
}
