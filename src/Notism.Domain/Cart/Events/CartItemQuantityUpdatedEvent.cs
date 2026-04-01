using Notism.Domain.Common;

namespace Notism.Domain.Cart.Events;

public class CartItemQuantityUpdatedEvent : DomainEvent
{
    public Guid CartItemId { get; }
    public Guid UserId { get; }
    public Guid FoodId { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    public CartItemQuantityUpdatedEvent(
        Guid cartItemId,
        Guid userId,
        Guid foodId,
        int oldQuantity,
        int newQuantity)
    {
        CartItemId = cartItemId;
        UserId = userId;
        FoodId = foodId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }
}