using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.User.Specifications;

public class UserByIdSpecification : Specification<User>
{
    private readonly Guid _userId;

    public UserByIdSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Id == _userId;
    }
}