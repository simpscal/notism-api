using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.User.Specifications;

public class ExpiredPasswordResetTokensSpecification : Specification<PasswordResetToken>
{
    private readonly DateTime _cutoffDate;

    public ExpiredPasswordResetTokensSpecification(DateTime cutoffDate)
    {
        _cutoffDate = cutoffDate;
    }

    public override Expression<Func<PasswordResetToken, bool>> ToExpression()
    {
        return prt => prt.ExpiresAt < _cutoffDate || prt.IsUsed;
    }
}