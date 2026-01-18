using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.User.Specifications;

public class PasswordResetTokenByTokenSpecification : Specification<PasswordResetToken>
{
    private readonly string _token;

    public PasswordResetTokenByTokenSpecification(string token)
    {
        _token = token;
    }

    public override Expression<Func<PasswordResetToken, bool>> ToExpression()
    {
        return token => token.Token == _token && !token.IsUsed && token.ExpiresAt > DateTime.UtcNow;
    }
}