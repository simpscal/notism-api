using System.Linq.Expressions;

using Order.Domain.Common.Specifications;

namespace Order.Domain.User.Specifications;

public class ExistingUserSpecification(string email) : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Email.ToLower() == email.ToLower();
    }
}