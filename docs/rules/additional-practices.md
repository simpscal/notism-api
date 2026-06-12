# Additional Best Practices

## Additional Best Practices

### Aggregate Roots

**✅ DO: Make Entities Aggregate Roots When Managed Independently**

```csharp
// If CartItem is managed independently with its own repository
public class CartItem : AggregateRoot { }

// If CartItem is part of a Cart aggregate
public class Cart : AggregateRoot
{
    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
}
```

### Value Objects

**✅ DO: Use Value Objects for Domain Concepts**

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsValidEmail(value))
            throw new ArgumentException("Invalid email address", nameof(value));
        Value = value;
    }

    public static Email Create(string value) => new(value);
}
```

### Private Constructors

**✅ DO: Use Private Constructors with Factory Methods**

```csharp
public class CartItem : AggregateRoot
{
    private CartItem(Guid userId, Guid foodId, int quantity)
    {
        // Initialize...
    }

    public static CartItem Create(Guid userId, Guid foodId, int quantity)
    {
        // Validation
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        return new CartItem(userId, foodId, quantity);
    }
}
```

### Include Related Entities

**✅ DO: Include Required Navigation Properties Inline in the Read**

A read whose response maps off a navigation graph declares those navigations with
`.Include(...)` directly on the `IReadDbContext.Set<T>()` chain in the handler:

```csharp
var cartItems = await _readDbContext.Set<CartItem>()
    .Where(c => c.UserId == userId)
    .Include(c => c.Food)
    .Include(c => c.Food.Images)
    .ToListAsync(cancellationToken);
```

---

## Summary

Following these best practices ensures:

- **Maintainability**: Code is organized and easy to understand
- **Testability**: Components can be tested independently
- **Consistency**: Patterns are applied uniformly across the codebase
- **Scalability**: Architecture supports growth and change
- **Quality**: Business rules are properly enforced and validated

