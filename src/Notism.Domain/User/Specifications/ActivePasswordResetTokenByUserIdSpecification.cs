using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.User.Specifications;

public class ActivePasswordResetTokenByUserIdSpecification : Specification<PasswordResetToken>
{
    private readonly Guid _userId;

    public ActivePasswordResetTokenByUserIdSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override Expression<Func<PasswordResetToken, bool>> ToExpression()
    {
        return token => token.UserId == _userId && !token.IsUsed && token.ExpiresAt > DateTime.UtcNow;
    }
}