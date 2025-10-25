using System.Linq.Expressions;

using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.RefreshToken.Specifications;

public class RefreshTokenByTokenSpecification : ISpecification<RefreshToken>
{
    private readonly string _token;

    public RefreshTokenByTokenSpecification(string token)
    {
        _token = token;
    }

    public Expression<Func<RefreshToken, bool>> ToExpression()
    {
        return refreshToken => refreshToken.Token == _token;
    }

    public bool IsSatisfiedBy(RefreshToken entity)
    {
        return entity.Token == _token;
    }
}