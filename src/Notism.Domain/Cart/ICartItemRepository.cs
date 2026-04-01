using Notism.Domain.Common.Interfaces;

namespace Notism.Domain.Cart;

public interface ICartItemRepository : IRepository<CartItem>
{
    Task ClearCart(Guid userId);
}