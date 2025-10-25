namespace BuildingBlocks.SharedServices.Implementations;

public class OtpService : IOtpService
{
	public string GenerateOtp(int length = 6)
	{
		using (var rng = RandomNumberGenerator.Create())
		{
			byte[] tokenData = new byte[length];
			rng.GetBytes(tokenData);

			string otp = string.Empty;
			for (int i = 0; i < length; i++)
			{
				otp += (tokenData[i] % 10).ToString();
			}
			return otp;
		}
	}
}
