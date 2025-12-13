using Notism.Domain.Common;

namespace Notism.Domain.Period;

public class Period : AggregateRoot
{
    public string Name { get; private set; }
    public int StartYear { get; private set; }
    public int EndYear { get; private set; }
    public string? Description { get; private set; }
    public Guid? ThumbnailMediaAssetId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }

    private Period(string name, int startYear, int endYear, string? description = null, Guid? thumbnailMediaAssetId = null, int displayOrder = 0, bool isPublished = false, Guid? createdBy = null)
    {
        Name = name;
        StartYear = startYear;
        EndYear = endYear;
        Description = description;
        ThumbnailMediaAssetId = thumbnailMediaAssetId;
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }

    public static Period Create(string name, int startYear, int endYear, string? description = null, Guid? thumbnailMediaAssetId = null, int displayOrder = 0, bool isPublished = false, Guid? createdBy = null)
    {
        if (startYear > endYear)
        {
            throw new ArgumentException("StartYear must be less than or equal to EndYear", nameof(startYear));
        }

        return new Period(name, startYear, endYear, description, thumbnailMediaAssetId, displayOrder, isPublished, createdBy);
    }

    public void Update(string name, int startYear, int endYear, string? description = null, Guid? thumbnailMediaAssetId = null, int displayOrder = 0, Guid? updatedBy = null)
    {
        if (startYear > endYear)
        {
            throw new ArgumentException("StartYear must be less than or equal to EndYear", nameof(startYear));
        }

        Name = name;
        StartYear = startYear;
        EndYear = endYear;
        Description = description;
        ThumbnailMediaAssetId = thumbnailMediaAssetId;
        DisplayOrder = displayOrder;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish(Guid? updatedBy = null)
    {
        IsPublished = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish(Guid? updatedBy = null)
    {
        IsPublished = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}