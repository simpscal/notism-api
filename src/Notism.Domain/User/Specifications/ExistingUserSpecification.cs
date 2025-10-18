using System.Linq.Expressions;
using Notism.Domain.Common.Specifications;
using Notism.Domain.User.ValueObjects;

namespace Notism.Domain.User.Specifications;

public class ExistingUserSpecification : Specification<User>
{
    private readonly Email _email;

    public ExistingUserSpecification(string email)
    {
        _email = Email.Create(email);
    }

    public ExistingUserSpecification(Email email)
    {
        _email = email;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Email.Value.ToLower() == _email.Value.ToLower();
    }
}