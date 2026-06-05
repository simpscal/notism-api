# Domain Events

## Domain Events

### When to Raise Domain Events

**✅ DO: Raise Events for Important Business State Changes**

```csharp
public class CartItem : AggregateRoot
{
    private CartItem(Guid userId, Guid foodId, int quantity)
    {
        UserId = userId;
        FoodId = foodId;
        Quantity = quantity;
        
        AddDomainEvent(new CartItemAddedEvent(Id, UserId, FoodId, Quantity));
    }

    public void UpdateQuantity(int quantity)
    {
        var oldQuantity = Quantity;
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;

        ClearDomainEvents();
        AddDomainEvent(new CartItemQuantityUpdatedEvent(Id, UserId, FoodId, oldQuantity, quantity));
    }
}
```

**✅ DO: Inherit from DomainEvent Base Class**

```csharp
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
```

**❌ DON'T: Raise Events for Every Property Change**

Only raise events for:
- Significant business events (created, updated, deleted)
- Events that other aggregates or bounded contexts need to know about
- Events that trigger side effects (notifications, logging, etc.)

---

