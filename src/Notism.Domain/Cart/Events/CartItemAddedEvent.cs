using Notism.Domain.Common;

namespace Notism.Domain.Cart.Events;

public class CartItemAddedEvent : DomainEvent
{
    public Guid CartItemId { get; }
    public Guid UserId { get; }
    public Guid FoodId { get; }
    public int Quantity { get; }

    public CartItemAddedEvent(Guid cartItemId, Guid userId, Guid foodId, int quantity)
    {
        CartItemId = cartItemId;
        UserId = userId;
        FoodId = foodId;
        Quantity = quantity;
    }
}

