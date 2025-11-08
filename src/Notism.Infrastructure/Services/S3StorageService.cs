using Amazon.S3;
using Amazon.S3.Model;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Notism.Application.Common.Interfaces;

namespace Notism.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3StorageService> _logger;
    private readonly string _bucketName;

    public S3StorageService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = configuration["AWS:BucketName"] ?? throw new ArgumentNullException("AWS:BucketName configuration is missing");
    }

    public async Task<(string Url, string Key)> GeneratePresignedUploadUrlAsync(string fileName, string contentType, int expirationMinutes = 60)
    {
        try
        {
            var key = $"uploads/{Guid.NewGuid()}/{fileName}";

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
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
                BucketName = _bucketName,
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

    public async Task<bool> DeleteFileAsync(string fileKey)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
            };

            await _s3Client.DeleteObjectAsync(request);

            _logger.LogInformation("Deleted file: {FileKey}", fileKey);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileKey}", fileKey);
            return false;
        }
    }
}
