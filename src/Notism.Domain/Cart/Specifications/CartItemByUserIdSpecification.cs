using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Cart.Specifications;

public class CartItemByUserIdSpecification : Specification<CartItem>
{
    private readonly Guid _userId;

    public CartItemByUserIdSpecification(Guid userId)
    {
        _userId = userId;
        Include(c => c.Food);
        Include(c => c.Food.Images);
    }

    public override Expression<Func<CartItem, bool>> ToExpression()
    {
        return cartItem => cartItem.UserId == _userId;
    }
}

