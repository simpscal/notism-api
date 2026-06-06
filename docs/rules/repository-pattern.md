# Repository Pattern

## Repository Pattern

### Use Specifications with IRepository<T> Methods

**✅ DO: Use Specifications with IRepository<T> Methods**

```csharp
// In Handler
var specification = new CartItemByUserIdSpecification(userId);
var cartItems = await _cartItemRepository.FilterByExpressionAsync(specification);

// Repository interface stays clean
public interface ICartItemRepository : IRepository<CartItem>
{
    // No custom query methods needed for simple queries
}
```

**❌ DON'T: Add Custom Query Methods to Repository Interfaces**

```csharp
// Avoid this pattern for simple queries
public interface ICartItemRepository : IRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId);
    Task<CartItem?> FindCartItemByUserAndFoodAsync(Guid userId, Guid foodId);
}
```

**When to Add Custom Repository Methods**

Only add custom methods to repository interfaces when:
- The operation involves complex business logic that belongs in the repository
- The operation is a specific domain operation (e.g., `ClearCartByUserIdAsync`)
- The operation cannot be easily expressed as a specification

```csharp
// ✅ Acceptable: Complex operation that encapsulates business logic
public interface ICartItemRepository : IRepository<CartItem>
{
    Task ClearCartByUserIdAsync(Guid userId); // Uses specifications internally
}
```

### Why Use Specifications?

1. **Separation of Concerns**: Query logic lives in Application layer as specifications, not Infrastructure
2. **Reusability**: Specifications can be composed and reused across different queries
3. **Testability**: Specifications are easy to unit test independently
4. **Consistency**: All repositories follow the same pattern using `IRepository<T>`
5. **Flexibility**: Easy to combine specifications using `And()`, `Or()`, `Not()` methods
6. **Maintainability**: Query logic is centralized in specification classes, not scattered in repository methods

### Persistence Commit Policy

**Single commit point.** A handler that mutates state commits exactly once. The target policy is to commit through `IUnitOfWork` and to keep `SaveChangesAsync` off the handler-facing repository surface so a double-save is impossible.

Until the full migration lands, handlers must still issue at most one commit per logical operation and must not call `SaveChangesAsync` on more than one repository sharing the same `DbContext`.

---

