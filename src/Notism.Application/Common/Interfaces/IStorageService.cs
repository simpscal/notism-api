namespace Notism.Application.Common.Interfaces;

public interface IStorageService
{
    Task<(string Url, string Key)> GeneratePresignedUploadUrlAsync(string folderName, string fileName, string contentType, int expirationMinutes = 60);
    Task<string> GeneratePresignedDownloadUrlAsync(string fileKey, int expirationMinutes = 60);

    /// <summary>
    /// Returns the public URL for a file. Prepends the configured storage prefix for the given type when the fileKey has no prefix (as stored in DB).
    /// </summary>
    /// <param name="fileKey">File key as stored in database (without prefix) or legacy key with prefix.</param>
    /// <param name="storageType">Storage type: avatar, food, or food-detail. Used to resolve the prefix from configuration.</param>
    string GetPublicUrl(string fileKey, string storageType);

    /// <summary>
    /// Deletes a file from the private bucket. Prepends the configured prefix for the given type to resolve the S3 key.
    /// </summary>
    /// <param name="fileKey">File key as stored in database (without prefix).</param>
    /// <param name="storageType">Storage type: avatar or food. Used to resolve the prefix from configuration.</param>
    Task<bool> DeleteFileAsync(string fileKey, string storageType);
}