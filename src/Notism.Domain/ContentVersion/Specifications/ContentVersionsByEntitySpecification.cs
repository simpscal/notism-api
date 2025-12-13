using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.ContentVersion.Specifications;

public class ContentVersionsByEntitySpecification : Specification<ContentVersion>
{
    private readonly string _entityType;
    private readonly Guid _entityId;

    public ContentVersionsByEntitySpecification(string entityType, Guid entityId)
    {
        _entityType = entityType;
        _entityId = entityId;
    }

    public override Expression<Func<ContentVersion, bool>> ToExpression()
    {
        return cv => cv.EntityType == _entityType && cv.EntityId == _entityId && !cv.IsDeleted;
    }
}

