# Repository Pattern

`IRepository<T>` is the **command/write boundary** for an aggregate. It loads tracked
aggregates for mutation, adds/removes entities, and commits. Read-only and projection
queries do **not** live here — see [Read Queries](read-queries.md).

## Write-path loads

Read-modify-write handlers load a **tracked** aggregate by a richer-than-PK predicate
(optionally with a navigation graph to mutate), change it, then `SaveChanges`:

```csharp
// Load tracked, mutate, save
var order = await _orderRepository.GetForUpdateAsync(
    o => o.Id == request.OrderId && o.UserId == request.UserId);

order.Cancel();
await _orderRepository.SaveChangesAsync();
```

`IRepository<T>` exposes:

- `GetForUpdateAsync(predicate, includes?)` — a single tracked aggregate.
- `ListForUpdateAsync(predicate, includes?)` — every matching tracked aggregate (bulk
  read-modify-write).
- `AddAsync` / `Add` / `Remove` / `SaveChangesAsync`.

The optional `includes` declares the navigation graph to load for mutation via the
`IncludeBuilder<T>` (typed lambdas and/or string paths).

**❌ There is no specification layer.** Do not add `FindByExpression`,
`FilterByExpression`, `FilterPaged` or specification classes — they have been removed.

**❌ Don't add read/projection methods to repository interfaces.** A read is an
Application query object over `IReadDbContext`, not a repository method.

```csharp
// Avoid — reads are query objects, not repo methods
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
