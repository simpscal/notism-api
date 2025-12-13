using System.Text.Json;

using Notism.Domain.Common;

namespace Notism.Domain.AuditLog;

public class AuditLog : Entity
{
    public Guid? UserId { get; private set; }
    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public JsonDocument? Changes { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditLog(Guid? userId, string action, string entityType, Guid? entityId = null, JsonDocument? changes = null, string? ipAddress = null, string? userAgent = null)
    {
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        Changes = changes;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public static AuditLog Create(Guid? userId, string action, string entityType, Guid? entityId = null, JsonDocument? changes = null, string? ipAddress = null, string? userAgent = null)
    {
        return new AuditLog(userId, action, entityType, entityId, changes, ipAddress, userAgent);
    }
}