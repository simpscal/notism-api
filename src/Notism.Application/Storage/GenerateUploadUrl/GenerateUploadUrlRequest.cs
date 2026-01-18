using System.Text.Json.Serialization;

using MediatR;

using Notism.Shared.Attributes;

namespace Notism.Application.Storage.GenerateUploadUrl;

public class GenerateUploadUrlRequest : IRequest<GenerateUploadUrlResponse>
{
    [JsonIgnore]
    public string Type { get; set; } = string.Empty;
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    [JsonIgnore]
    public int ExpirationMinutes { get; set; } = 60;
}

public enum UploadType
{
    [StringValue("avatar")]
    Avatar,
    [StringValue("upload")]
    Upload,
}