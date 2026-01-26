using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Cart.Specifications;

public class CartItemByIdSpecification : Specification<CartItem>
{
    private readonly Guid _cartItemId;

    public CartItemByIdSpecification(Guid cartItemId)
    {
        _cartItemId = cartItemId;
    }

    public override Expression<Func<CartItem, bool>> ToExpression()
    {
        return cartItem => cartItem.Id == _cartItemId;
    }
}

