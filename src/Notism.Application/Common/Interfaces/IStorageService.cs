namespace Notism.Application.Common.Interfaces;

public interface IStorageService
{
    Task<(string Url, string Key)> GeneratePresignedUploadUrlAsync(string fileName, string contentType, int expirationMinutes = 60);
    Task<string> GeneratePresignedDownloadUrlAsync(string fileKey, int expirationMinutes = 60);
    string GetPublicUrl(string fileKey);
    Task<bool> DeleteFileAsync(string fileKey);
}
