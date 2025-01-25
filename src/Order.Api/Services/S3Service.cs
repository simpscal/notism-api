using Amazon.S3;
using Amazon.S3.Model;

using Order.Shared.Constants;
using Order.Shared.Interfaces;
using Order.Shared.Utilities;

namespace Order.Api.Services;

public class S3Service(IAmazonS3 s3Client, IConfiguration configuration) : IFileStorageService
{
    public Task<string> GetPresignedUrlAsync(
        string fileName,
        string folderName,
        string contentType,
        TimeSpan duration)
    {
        if (!FileValidatorUtility.IsValidFileName(fileName))
        {
            throw new Exception("File name is invalid");
        }

        if (!FileContentTypes.All.Contains(contentType))
        {
            throw new Exception("Content type is invalid");
        }

        var request = new GetPreSignedUrlRequest
        {
            BucketName = configuration["AWS:BucketName"],
            Key = $"{folderName}/{fileName}",
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(duration),
        };

        return s3Client.GetPreSignedURLAsync(request);
    }
}