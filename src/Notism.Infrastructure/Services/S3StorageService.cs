using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Notism.Application.Common.Constants;
using Notism.Application.Common.Interfaces;
using Notism.Shared.Configuration;

namespace Notism.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private static readonly HashSet<string> ValidStorageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        StorageTypeConstants.Avatar,
        StorageTypeConstants.Food,
        StorageTypeConstants.FoodDetail,
    };

    private readonly IAmazonS3 _s3Client;
    private readonly AwsSettings _awsSettings;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<AwsSettings> awsSettings,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _awsSettings = awsSettings.Value;
        _logger = logger;
    }

    public async Task<(string Url, string Key)> GeneratePresignedUploadUrlAsync(string folderName, string fileName, string contentType, int expirationMinutes = 60)
    {
        try
        {
            var key = $"{folderName}/{Guid.NewGuid()}/{fileName}";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _awsSettings.PrivateBucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                ContentType = contentType,
            };

            var url = await Task.FromResult(_s3Client.GetPreSignedURL(request));

            _logger.LogInformation("Generated presigned upload URL for file: {FileName} with key: {Key}", fileName, key);

            return (url, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned upload URL for file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GeneratePresignedDownloadUrlAsync(string fileKey, int expirationMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _awsSettings.PrivateBucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            };

            var url = await Task.FromResult(_s3Client.GetPreSignedURL(request));

            _logger.LogInformation("Generated presigned download URL for file: {FileKey}", fileKey);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned download URL for file: {FileKey}", fileKey);
            throw;
        }
    }

    public string GetPublicUrl(string fileKey, string storageType)
    {
        if (string.IsNullOrEmpty(fileKey))
        {
            return string.Empty;
        }

        var fullKey = GetFullS3Key(fileKey, storageType);
        return $"https://{_awsSettings.PublicBucketName}.s3.{_awsSettings.Region}.amazonaws.com/{fullKey}";
    }

    public async Task<bool> DeleteFileAsync(string fileKey, string storageType)
    {
        try
        {
            var fullKey = GetFullS3Key(fileKey, storageType);
            if (string.IsNullOrEmpty(fullKey))
            {
                return false;
            }

            var request = new DeleteObjectRequest
            {
                BucketName = _awsSettings.PrivateBucketName,
                Key = fullKey,
            };

            await _s3Client.DeleteObjectAsync(request);

            _logger.LogInformation("Deleted file: {FileKey}", fullKey);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileKey}", fileKey);
            return false;
        }
    }

    private static string GetFullS3Key(string fileKey, string storageType)
    {
        if (!ValidStorageTypes.Contains(storageType))
        {
            throw new ArgumentException($"Unknown storage type: {storageType}", nameof(storageType));
        }

        var prefixWithSlash = storageType + "/";
        if (string.IsNullOrEmpty(fileKey))
        {
            return string.Empty;
        }

        if (fileKey.StartsWith(prefixWithSlash, StringComparison.Ordinal))
        {
            return fileKey;
        }

        return prefixWithSlash + fileKey;
    }
}