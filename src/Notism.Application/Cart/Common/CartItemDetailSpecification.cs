using System.Linq.Expressions;

using Notism.Domain.Cart;
using Notism.Domain.Common.Specifications;

namespace Notism.Application.Cart.Common;

/// <summary>
/// Loads a cart item with the full graph required to build a cart-item response
/// (food, primary image, category, customisation groups and options, and the
/// item's own customisations). Encapsulates the shared include chain reused by
/// the cart read/update handlers so it is defined in exactly one place.
/// </summary>
public class CartItemDetailSpecification : Specification<CartItem>
{
    private readonly Expression<Func<CartItem, bool>> _filter;

    public CartItemDetailSpecification(Expression<Func<CartItem, bool>> filter)
    {
        _filter = filter;

        Include(c => c.Food);
        Include("Food.Category");
        Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1));
        Include("Food.CustomisationGroups.Options");
        Include(c => c.Customisations);
    }

    public override Expression<Func<CartItem, bool>> ToExpression()
    {
        return _filter;
    }
}