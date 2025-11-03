using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.RefreshToken.Specifications;

public class RefreshTokenByTokenSpecification : Specification<RefreshToken>
{
    private readonly string _token;

    public RefreshTokenByTokenSpecification(string token)
    {
        _token = token;
    }

    public override Expression<Func<RefreshToken, bool>> ToExpression()
    {
        return refreshToken => refreshToken.Token == _token;
    }
}