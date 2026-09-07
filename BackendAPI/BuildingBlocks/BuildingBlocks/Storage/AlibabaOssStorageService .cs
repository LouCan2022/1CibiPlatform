using Aliyun.OSS;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Storage;

public sealed class AlibabaOssStorageService : IObjectStorageService
{
	private readonly OssClient _client;
	private readonly IConfiguration _configuration;
	private readonly string OssBucket = string.Empty;
	private readonly string _atsTestFolder;
	public AlibabaOssStorageService(
		OssClient client,
		IConfiguration configuration)
	{
		_client = client;
		_configuration = configuration;
		OssBucket = _configuration
					 .GetSection("AlibabaOss")
					 .GetValue<string>("BucketName") ?? "one-cibi";
		_atsTestFolder = _configuration
						.GetSection("AlibabaOss")
						.GetValue<string>("ATSTestFolder", "");
	}

	public async Task<string> UploadAsync(
		string folderName,
		string fileName,
		Stream stream,		
		CancellationToken ct = default)
	{
		ArgumentNullException.ThrowIfNull(stream);

		// The GUID keeps keys unique even when two uploads share a filename;
		// without it a same-named upload silently overwrites the other object
		// while both DB rows keep pointing at the one surviving key. The test
		// folder is therefore only a prefix, never a replacement for the key.
		var uniqueKey = $"{folderName}/{Guid.CreateVersion7():N}-{fileName}";
		var objectKey = string.IsNullOrEmpty(_atsTestFolder)
			? uniqueKey
			: $"{_atsTestFolder.TrimEnd('/')}/{uniqueKey}";

		await Task.Run(() => { 
			ct.ThrowIfCancellationRequested(); 
			_client.PutObject(
				OssBucket, 
				objectKey, 
				stream); 
		}, ct);

		return objectKey;
	}

	public async Task<Stream> DownloadAsync(
		string objectKey,
		CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

		var result = await Task.Run(() =>
		{
			ct.ThrowIfCancellationRequested();

			return _client.GetObject(
				OssBucket,
				objectKey);
		}, ct);

		// caller disposes stream
		return result.Content;
	}

	public async Task DeleteAsync(
		string objectKey,
		CancellationToken ct = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

		await Task.Run(() =>
		{
			ct.ThrowIfCancellationRequested();

			_client.DeleteObject(
				OssBucket,
				objectKey);
		}, ct);
	}

	public Task<string> GenerateDownloadUrlAsync(
		string objectKey,
		TimeSpan expiry)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(objectKey);

		var expiration = DateTime.UtcNow.Add(expiry);

		var uri = _client.GeneratePresignedUri(
			OssBucket,
			objectKey,
			expiration);

		return Task.FromResult(uri.ToString());
	}
}