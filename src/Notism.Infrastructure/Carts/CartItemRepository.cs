using Notism.Domain.Cart;
using Notism.Domain.Common.Specifications;
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
        var specification = new FilterSpecification<CartItem>(c => c.UserId == userId);
        var cartItems = await FilterByExpressionAsync(specification);

        foreach (var item in cartItems)
        {
            Remove(item);
        }
    }
}