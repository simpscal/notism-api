using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User.Specifications;

public class UserByEmailSpecification : Specification<User>
{
    private readonly Email _email;

    public UserByEmailSpecification(string email)
    {
        _email = Email.Create(email);
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Email.Equals(_email);
    }
}