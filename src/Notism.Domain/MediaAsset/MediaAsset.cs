using Notism.Domain.Common;

namespace Notism.Domain.MediaAsset;

public class MediaAsset : AggregateRoot
{
    public string FileName { get; private set; }
    public string StoragePath { get; private set; }
    public string ContentType { get; private set; }
    public MediaType MediaType { get; private set; }
    public long FileSize { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public int? Duration { get; private set; }
    public string? ThumbnailPath { get; private set; }
    public string? AltText { get; private set; }
    public string? Caption { get; private set; }
    public List<string> Tags { get; private set; } = new();

    private MediaAsset(string fileName, string storagePath, string contentType, MediaType mediaType, long fileSize, int? width = null, int? height = null, int? duration = null, string? thumbnailPath = null, string? altText = null, string? caption = null, List<string>? tags = null, Guid? createdBy = null)
    {
        FileName = fileName;
        StoragePath = storagePath;
        ContentType = contentType;
        MediaType = mediaType;
        FileSize = fileSize;
        Width = width;
        Height = height;
        Duration = duration;
        ThumbnailPath = thumbnailPath;
        AltText = altText;
        Caption = caption;
        Tags = tags ?? new List<string>();
        CreatedBy = createdBy;
    }

    public static MediaAsset Create(string fileName, string storagePath, string contentType, MediaType mediaType, long fileSize, int? width = null, int? height = null, int? duration = null, string? thumbnailPath = null, string? altText = null, string? caption = null, List<string>? tags = null, Guid? createdBy = null)
    {
        if (fileSize <= 0)
        {
            throw new ArgumentException("FileSize must be greater than zero", nameof(fileSize));
        }

        // Validate MediaType matches ContentType category
        var contentTypeLower = contentType.ToLowerInvariant();
        var isValidType = mediaType switch
        {
            MediaType.Image => contentTypeLower.StartsWith("image/"),
            MediaType.Animation => contentTypeLower.StartsWith("image/") || contentTypeLower.StartsWith("video/"),
            MediaType.Video => contentTypeLower.StartsWith("video/"),
            _ => false,
        };

        if (!isValidType)
        {
            throw new ArgumentException($"ContentType {contentType} does not match MediaType {mediaType}", nameof(contentType));
        }

        if (width.HasValue && !height.HasValue)
        {
            throw new ArgumentException("Height must be provided when Width is provided", nameof(height));
        }

        return new MediaAsset(fileName, storagePath, contentType, mediaType, fileSize, width, height, duration, thumbnailPath, altText, caption, tags, createdBy);
    }

    public void Update(string? altText = null, string? caption = null, List<string>? tags = null)
    {
        AltText = altText;
        Caption = caption;
        Tags = tags ?? new List<string>();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}

public enum MediaType
{
    Image,
    Animation,
    Video,
}