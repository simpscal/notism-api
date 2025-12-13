using System.Text.RegularExpressions;

using Notism.Domain.Common;

namespace Notism.Domain.Blog;

public class Blog : AggregateRoot
{
    public string Title { get; private set; }
    public string Slug { get; private set; }
    public string? ShortDescription { get; private set; }
    public string Content { get; private set; }
    public Guid? AuthorId { get; private set; }
    public string? AuthorName { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public Guid? ThumbnailMediaAssetId { get; private set; }
    public bool IsPublished { get; private set; }
    public int ViewCount { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public List<string> Tags { get; private set; } = new();

    private Blog(string title, string slug, string content, string? shortDescription = null, Guid? authorId = null, string? authorName = null, Guid? thumbnailMediaAssetId = null, bool isPublished = false, string? metaTitle = null, string? metaDescription = null, List<string>? tags = null, Guid? createdBy = null)
    {
        Title = title;
        Slug = slug;
        Content = content;
        ShortDescription = shortDescription;
        AuthorId = authorId;
        AuthorName = authorName;
        ThumbnailMediaAssetId = thumbnailMediaAssetId;
        IsPublished = isPublished;
        ViewCount = 0;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        Tags = tags ?? new List<string>();
        CreatedBy = createdBy;
        UpdatedBy = createdBy;

        if (isPublished)
        {
            PublishedAt = DateTime.UtcNow;
        }
    }

    public static Blog Create(string title, string slug, string content, string? shortDescription = null, Guid? authorId = null, string? authorName = null, Guid? thumbnailMediaAssetId = null, bool isPublished = false, string? metaTitle = null, string? metaDescription = null, List<string>? tags = null, Guid? createdBy = null)
    {
        // Validate slug format (URL-friendly)
        if (!Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new ArgumentException("Slug must be URL-friendly (lowercase, hyphens, alphanumeric)", nameof(slug));
        }

        return new Blog(title, slug, content, shortDescription, authorId, authorName, thumbnailMediaAssetId, isPublished, metaTitle, metaDescription, tags, createdBy);
    }

    public void Update(string title, string slug, string content, string? shortDescription = null, Guid? authorId = null, string? authorName = null, Guid? thumbnailMediaAssetId = null, string? metaTitle = null, string? metaDescription = null, List<string>? tags = null, Guid? updatedBy = null)
    {
        if (!Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
        {
            throw new ArgumentException("Slug must be URL-friendly (lowercase, hyphens, alphanumeric)", nameof(slug));
        }

        Title = title;
        Slug = slug;
        Content = content;
        ShortDescription = shortDescription;
        AuthorId = authorId;
        AuthorName = authorName;
        ThumbnailMediaAssetId = thumbnailMediaAssetId;
        MetaTitle = metaTitle;
        MetaDescription = metaDescription;
        Tags = tags ?? new List<string>();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish(Guid? updatedBy = null)
    {
        IsPublished = true;
        if (!PublishedAt.HasValue)
        {
            PublishedAt = DateTime.UtcNow;
        }

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish(Guid? updatedBy = null)
    {
        IsPublished = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}