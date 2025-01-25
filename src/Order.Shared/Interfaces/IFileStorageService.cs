namespace Order.Shared.Interfaces;

public interface IFileStorageService
{
    public Task<string> GetPresignedUrlAsync(
        string fileName,
        string folderName,
        string contentType,
        TimeSpan duration);
}