using System.Linq.Expressions;

using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.User.Specifications;

public class ExpiredPasswordResetTokensSpecification : ISpecification<PasswordResetToken>
{
    private readonly DateTime _cutoffDate;

    public ExpiredPasswordResetTokensSpecification(DateTime cutoffDate)
    {
        _cutoffDate = cutoffDate;
    }

    public Expression<Func<PasswordResetToken, bool>> ToExpression()
    {
        return prt => prt.ExpiresAt < _cutoffDate || prt.IsUsed;
    }

    public bool IsSatisfiedBy(PasswordResetToken entity)
    {
        return entity.ExpiresAt < _cutoffDate || entity.IsUsed;
    }
}