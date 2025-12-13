using Notism.Domain.Common;

namespace Notism.Domain.BlogMedia;

public class BlogMedia : Entity
{
    public Guid BlogId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsFeatured { get; private set; }
    public BlogMediaUsageType UsageType { get; private set; }

    private BlogMedia(Guid blogId, Guid mediaAssetId, int displayOrder = 0, bool isFeatured = false, BlogMediaUsageType usageType = BlogMediaUsageType.Content)
    {
        BlogId = blogId;
        MediaAssetId = mediaAssetId;
        DisplayOrder = displayOrder;
        IsFeatured = isFeatured;
        UsageType = usageType;
    }

    public static BlogMedia Create(Guid blogId, Guid mediaAssetId, int displayOrder = 0, bool isFeatured = false, BlogMediaUsageType usageType = BlogMediaUsageType.Content)
    {
        return new BlogMedia(blogId, mediaAssetId, displayOrder, isFeatured, usageType);
    }

    public void Update(int displayOrder, bool isFeatured, BlogMediaUsageType usageType)
    {
        DisplayOrder = displayOrder;
        IsFeatured = isFeatured;
        UsageType = usageType;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum BlogMediaUsageType
{
    Content,
    Header,
    Thumbnail,
}