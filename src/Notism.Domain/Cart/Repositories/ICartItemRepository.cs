using Notism.Domain.Cart;
using Notism.Domain.Common.Repositories;

namespace Notism.Domain.Cart.Repositories;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task ClearCart(Guid userId);
}