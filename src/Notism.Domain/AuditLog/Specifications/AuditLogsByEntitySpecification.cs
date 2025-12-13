using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.AuditLog.Specifications;

public class AuditLogsByEntitySpecification : Specification<AuditLog>
{
    private readonly string _entityType;
    private readonly Guid _entityId;

    public AuditLogsByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }

    public override Expression<Func<AuditLog, bool>> ToExpression()
    {
        return al => al.EntityType == _entityType && al.EntityId == _entityId && !al.IsDeleted;
    }
}

