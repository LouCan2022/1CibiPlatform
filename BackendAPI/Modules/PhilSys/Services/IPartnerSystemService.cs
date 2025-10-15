namespace PhilSys.Services;
public interface IPartnerSystemService
{
	Task<string> GetAccessTokenAsync();
	//Task<PhilSysResponse> VerifyByNameDobAsync(string token, IdentityData data);
	//Task<PhilSysResponse> VerifyByPCNAsync(string token, IdentityData data);
	Task<string> GenerateLivenessLinkAsync(string externalUserId);
}
