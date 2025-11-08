using MediatR;

namespace Notism.Application.Storage.GenerateUploadUrl;

public class GenerateUploadUrlRequest : IRequest<GenerateUploadUrlResponse>
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public int ExpirationMinutes { get; set; } = 60;
}
