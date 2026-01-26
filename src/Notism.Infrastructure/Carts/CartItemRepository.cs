using Notism.Domain.Cart;
using Notism.Domain.Cart.Specifications;
using Notism.Infrastructure.Common;

namespace Notism.Infrastructure.Carts;

public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    public CartItemRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }

    public async Task ClearCart(Guid userId)
    {
        var specification = new CartItemByUserIdSpecification(userId);
        var cartItems = await FilterByExpressionAsync(specification);

        foreach (var item in cartItems)
        {
            Remove(item);
        }
    }
}

