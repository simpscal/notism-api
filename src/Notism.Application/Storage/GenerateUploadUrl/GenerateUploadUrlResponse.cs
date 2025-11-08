namespace Notism.Application.Storage.GenerateUploadUrl;

public class GenerateUploadUrlResponse
{
    public required string UploadUrl { get; set; }
    public required string FileKey { get; set; }
    public DateTime ExpiresAt { get; set; }
}
