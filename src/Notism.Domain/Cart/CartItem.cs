using Notism.Domain.Cart.Events;
using Notism.Domain.Common;

namespace Notism.Domain.Cart;

public class CartItem : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid FoodId { get; private set; }
    public int Quantity { get; private set; }
    public Domain.Food.Food Food { get; private set; } = null!;

    private CartItem(Guid userId, Guid foodId, int quantity)
    {
        UserId = userId;
        FoodId = foodId;
        Quantity = quantity;

        AddDomainEvent(new CartItemAddedEvent(Id, UserId, FoodId, Quantity));
    }

    public static CartItem Create(Guid userId, Guid foodId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        return new CartItem(userId, foodId, quantity);
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        var oldQuantity = Quantity;
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;

        ClearDomainEvents();
        AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, UserId, FoodId, oldQuantity, quantity));
    }
}

