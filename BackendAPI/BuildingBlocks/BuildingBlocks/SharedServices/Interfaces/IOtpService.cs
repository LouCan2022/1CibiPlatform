namespace BuildingBlocks.SharedServices.Interfaces;

public interface IOtpService
{
	string GenerateOtp(int length = 6);
}
