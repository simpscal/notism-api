using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.RefreshToken.Specifications;

public class ExpiredRefreshTokensSpecification : Specification<RefreshToken>
{
    private readonly DateTime _cutoffDate;

    public ExpiredRefreshTokensSpecification(DateTime cutoffDate)
    {
        _cutoffDate = cutoffDate;
    }

    public override Expression<Func<RefreshToken, bool>> ToExpression()
    {
        return rt => rt.ExpiresAt < _cutoffDate || rt.IsRevoked;
    }
}