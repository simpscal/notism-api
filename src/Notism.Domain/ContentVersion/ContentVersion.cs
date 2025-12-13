using System.Text.Json;

using Notism.Domain.Common;

namespace Notism.Domain.ContentVersion;

public class ContentVersion : Entity
{
    public string EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public int VersionNumber { get; private set; }
    public JsonDocument Content { get; private set; }
    public string? ChangeDescription { get; private set; }

    private ContentVersion(string entityType, Guid entityId, int versionNumber, JsonDocument content, string? changeDescription = null, Guid? createdBy = null)
    {
        EntityType = entityType;
        EntityId = entityId;
        VersionNumber = versionNumber;
        Content = content;
        ChangeDescription = changeDescription;
        CreatedBy = createdBy;
    }

    public static ContentVersion Create(string entityType, Guid entityId, int versionNumber, JsonDocument content, string? changeDescription = null, Guid? createdBy = null)
    {
        if (!IsValidEntityType(entityType))
        {
            throw new ArgumentException($"EntityType must be one of: Period, Event, Blog", nameof(entityType));
        }

        if (versionNumber <= 0)
        {
            throw new ArgumentException("VersionNumber must be greater than zero", nameof(versionNumber));
        }

        return new ContentVersion(entityType, entityId, versionNumber, content, changeDescription, createdBy);
    }

    private static bool IsValidEntityType(string entityType)
    {
        return entityType.Equals("Period", StringComparison.OrdinalIgnoreCase) ||
               entityType.Equals("Event", StringComparison.OrdinalIgnoreCase) ||
               entityType.Equals("Blog", StringComparison.OrdinalIgnoreCase);
    }
}