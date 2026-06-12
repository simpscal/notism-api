# Repository Pattern

`IRepository<T>` is the **write-only boundary** for an aggregate. It adds and removes
entities and commits. It does **not** retrieve — reads and tracked write-path loads come
from `IReadDbContext`, see [Read Queries](read-queries.md).

```csharp
public interface IRepository<T>
    where T : class
{
    Task<T> AddAsync(T entity);
    T Add(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}
```

## Write-path loads come from the read port

A read-modify-write handler loads its **tracked** aggregate via
`IReadDbContext.Set<T>(tracking: true)` — composing `.Where(...)` and any `.Include(...)`
for the navigation graph to mutate — then commits through the repository:

```csharp
var order = await _readDbContext.Set<DomainOrder>(tracking: true)
        .Where(o => o.Id == request.OrderId && o.UserId == request.UserId)
        .FirstOrDefaultAsync(cancellationToken)
    ?? throw new ResultFailureException(_messages.OrderNotFound);

order.Cancel();
await _orderRepository.SaveChangesAsync();
```

The same scoped `AppDbContext` backs both the read port and the repository, so the tracked
mutation persists on `SaveChangesAsync`.

**❌ No retrieval on repositories.** Do not add `GetForUpdateAsync`, `ListForUpdateAsync`,
or any read/projection method. Tracked write loads come from `Set<T>(tracking: true)`.

**❌ No specification layer.** There is no `FindByExpression`, `FilterByExpression`,
`FilterPaged`, or specification class.

```csharp
// Avoid — reads are not repo methods
public interface ICartItemRepository : IRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId);
}
```

## When to add a custom repository method

Only for write-side domain operations that encapsulate behaviour the generic surface
cannot express:

```csharp
// ✅ Acceptable: a write-side domain operation
public interface ICartItemRepository : IRepository<CartItem>
{
    Task ClearCart(Guid userId);
}
```

## Persistence Commit Policy

**Single commit point.** A handler that mutates state commits exactly once. The target
policy is to commit through `IUnitOfWork` and to keep `SaveChangesAsync` off the
handler-facing repository surface so a double-save is impossible.

Until the full migration lands, handlers must still issue at most one commit per logical
operation and must not call `SaveChangesAsync` on more than one repository sharing the
same `DbContext`.
