using Notism.Domain.Cart;

namespace Notism.Application.Cart.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityResponse
{
    public Guid Id { get; set; }

    public static UpdateCartItemQuantityResponse FromDomain(CartItem cartItem)
    {
        return new UpdateCartItemQuantityResponse
        {
            Id = cartItem.Id,
        };
    }
}