using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.AuditLog.Specifications;

public class AuditLogsByUserIdSpecification : Specification<AuditLog>
{
    private readonly Guid _userId;

    public AuditLogsByUserIdSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override Expression<Func<AuditLog, bool>> ToExpression()
    {
        return al => al.UserId == _userId && !al.IsDeleted;
    }
}

