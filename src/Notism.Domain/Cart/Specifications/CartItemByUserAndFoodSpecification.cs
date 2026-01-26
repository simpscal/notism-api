using System.Linq.Expressions;

using Notism.Domain.Common.Specifications;

namespace Notism.Domain.Cart.Specifications;

public class CartItemByUserAndFoodSpecification : Specification<CartItem>
{
    private readonly Guid _userId;
    private readonly Guid _foodId;

    public CartItemByUserAndFoodSpecification(Guid userId, Guid foodId)
    {
        _userId = userId;
        _foodId = foodId;
        Include(c => c.Food);
        Include(c => c.Food.Images);
    }

    public override Expression<Func<CartItem, bool>> ToExpression()
    {
        return cartItem => cartItem.UserId == _userId && cartItem.FoodId == _foodId;
    }
}

