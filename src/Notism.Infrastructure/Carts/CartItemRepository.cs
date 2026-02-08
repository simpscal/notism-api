using Microsoft.EntityFrameworkCore;

using Notism.Domain.Cart;
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
        await _dbSet
            .Where(c => c.UserId == userId)
            .ExecuteDeleteAsync();
    }
}