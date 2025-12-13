using Notism.Domain.Common;

namespace Notism.Domain.EventMedia;

public class EventMedia : Entity
{
    public Guid EventId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public EventMediaUsageType UsageType { get; private set; }

    private EventMedia(Guid eventId, Guid mediaAssetId, int displayOrder = 0, bool isPrimary = false, EventMediaUsageType usageType = EventMediaUsageType.General)
    {
        EventId = eventId;
        MediaAssetId = mediaAssetId;
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        UsageType = usageType;
    }

    public static EventMedia Create(Guid eventId, Guid mediaAssetId, int displayOrder = 0, bool isPrimary = false, EventMediaUsageType usageType = EventMediaUsageType.General)
    {
        return new EventMedia(eventId, mediaAssetId, displayOrder, isPrimary, usageType);
    }

    public void Update(int displayOrder, bool isPrimary, EventMediaUsageType usageType)
    {
        DisplayOrder = displayOrder;
        IsPrimary = isPrimary;
        UsageType = usageType;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum EventMediaUsageType
{
    General,
    Timeline,
    Map,
    Detail,
}