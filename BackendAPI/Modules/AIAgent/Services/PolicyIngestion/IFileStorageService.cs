namespace AIAgent.Services.PolicyIngestion;

public interface IFileStorageService
{
	Task<string> SaveFileAsync(byte[] fileBytes, string fileName, CancellationToken cancellationToken = default);
	string GetFileUrl(string fileName);
}
