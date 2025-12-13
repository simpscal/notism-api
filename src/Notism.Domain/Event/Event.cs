using Notism.Domain.Common;

namespace Notism.Domain.Event;

public class Event : AggregateRoot
{
    public Guid PeriodId { get; private set; }
    public string Title { get; private set; }
    public string? ShortDescription { get; private set; }
    public string? Description { get; private set; }
    public DateOnly? EventDate { get; private set; }
    public int EventYear { get; private set; }
    public int? EventMonth { get; private set; }
    public int? EventDay { get; private set; }
    public bool IsApproximateDate { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? LocationName { get; private set; }
    public Guid? ThumbnailMediaAssetId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPublished { get; private set; }

    private Event(Guid periodId, string title, int eventYear, string? shortDescription = null, string? description = null, DateOnly? eventDate = null, int? eventMonth = null, int? eventDay = null, bool isApproximateDate = true, decimal? latitude = null, decimal? longitude = null, string? locationName = null, Guid? thumbnailMediaAssetId = null, int displayOrder = 0, bool isPublished = false, Guid? createdBy = null)
    {
        PeriodId = periodId;
        Title = title;
        EventYear = eventYear;
        ShortDescription = shortDescription;
        Description = description;
        EventDate = eventDate;
        EventMonth = eventMonth;
        EventDay = eventDay;
        IsApproximateDate = isApproximateDate;
        Latitude = latitude;
        Longitude = longitude;
        LocationName = locationName;
        ThumbnailMediaAssetId = thumbnailMediaAssetId;
        DisplayOrder = displayOrder;
        IsPublished = isPublished;
        CreatedBy = createdBy;
        UpdatedBy = createdBy;
    }

    public static Event Create(Guid periodId, string title, int eventYear, string? shortDescription = null, string? description = null, DateOnly? eventDate = null, int? eventMonth = null, int? eventDay = null, bool isApproximateDate = true, decimal? latitude = null, decimal? longitude = null, string? locationName = null, Guid? thumbnailMediaAssetId = null, int displayOrder = 0, bool isPublished = false, Guid? createdBy = null)
    {
        if (eventMonth.HasValue && (eventMonth < 1 || eventMonth > 12))
        {
            throw new ArgumentException("EventMonth must be between 1 and 12", nameof(eventMonth));
        }

        if (eventDay.HasValue && (!eventMonth.HasValue || eventDay < 1 || eventDay > 31))
        {
            throw new ArgumentException("EventDay must be valid for the given month", nameof(eventDay));
        }

        if ((latitude.HasValue && !longitude.HasValue) || (!latitude.HasValue && longitude.HasValue))
        {
            throw new ArgumentException("Both Latitude and Longitude must be provided together", nameof(latitude));
        }

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
        {
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        }

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
        {
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));
        }

        return new Event(periodId, title, eventYear, shortDescription, description, eventDate, eventMonth, eventDay, isApproximateDate, latitude, longitude, locationName, thumbnailMediaAssetId, displayOrder, isPublished, createdBy);
    }

    public void Update(string title, int eventYear, string? shortDescription = null, string? description = null, DateOnly? eventDate = null, int? eventMonth = null, int? eventDay = null, bool isApproximateDate = true, decimal? latitude = null, decimal? longitude = null, string? locationName = null, Guid? thumbnailMediaAssetId = null, int displayOrder = 0, Guid? updatedBy = null)
    {
        if (eventMonth.HasValue && (eventMonth < 1 || eventMonth > 12))
        {
            throw new ArgumentException("EventMonth must be between 1 and 12", nameof(eventMonth));
        }

        if (eventDay.HasValue && (!eventMonth.HasValue || eventDay < 1 || eventDay > 31))
        {
            throw new ArgumentException("EventDay must be valid for the given month", nameof(eventDay));
        }

        if ((latitude.HasValue && !longitude.HasValue) || (!latitude.HasValue && longitude.HasValue))
        {
            throw new ArgumentException("Both Latitude and Longitude must be provided together", nameof(latitude));
        }

        if (latitude.HasValue && (latitude < -90 || latitude > 90))
        {
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        }

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
        {
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));
        }

        Title = title;
        EventYear = eventYear;
        ShortDescription = shortDescription;
        Description = description;
        EventDate = eventDate;
        EventMonth = eventMonth;
        EventDay = eventDay;
        IsApproximateDate = isApproximateDate;
        Latitude = latitude;
        Longitude = longitude;
        LocationName = locationName;
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