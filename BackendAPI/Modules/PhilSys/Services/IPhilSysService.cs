namespace PhilSys.Services;

public interface IPhilSysService
{
	Task<string> GetPhilsysTokenAsync(string clientId,
		string clientSecret);

	Task<BasicInformationOrPCNResponseDTO> PostBasicInformationAsync(string first_name,
		string middle_name,
		string last_name,
		string suffix,
		string birth_date,
		string bearer_token,
		string face_liveness_session_id);

	Task<BasicInformationOrPCNResponseDTO> PostPCNAsync(string value,
		string bearer_token,
		string face_liveness_session_id);

}
