using System.Linq.Expressions;

using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.RefreshToken.Specifications;

public class ExpiredRefreshTokensSpecification : ISpecification<RefreshToken>
{
    private readonly DateTime _cutoffDate;

    public ExpiredRefreshTokensSpecification(DateTime cutoffDate)
    {
        _cutoffDate = cutoffDate;
    }

    public Expression<Func<RefreshToken, bool>> ToExpression()
    {
        return rt => rt.ExpiresAt < _cutoffDate || rt.IsRevoked;
    }

    public bool IsSatisfiedBy(RefreshToken entity)
    {
        return entity.ExpiresAt < _cutoffDate || entity.IsRevoked;
    }
}