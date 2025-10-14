using BuildingBlocks.SharedServices.Interfaces;
using System.Security.Cryptography;

namespace BuildingBlocks.SharedServices.Implementations;

public class OtpService : IOtpService
{
	private readonly ILogger<OtpService> _logger;

	public OtpService(ILogger<OtpService> logger)
	{
		_logger = logger;
	}
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

			_logger.LogInformation($"OTP generated successfully");
			return otp;
		}
	}
}
