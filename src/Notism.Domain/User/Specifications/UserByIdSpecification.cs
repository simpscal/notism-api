using System.Linq.Expressions;
using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.User.Specifications;

public class UserByIdSpecification : ISpecification<User>
{
    private readonly Guid _userId;

    public UserByIdSpecification(Guid userId)
    {
        _userId = userId;
    }

    public Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Id == _userId;
    }

    public bool IsSatisfiedBy(User entity)
    {
        return entity.Id == _userId;
    }
}