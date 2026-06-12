# Read Queries

Reads and writes use two different persistence paths.

## Reads — composed inline in the handler over `IReadDbContext`

A read handler injects `IReadDbContext` and chains EF Core operators directly on
`Set<T>()`. There is **no** separate `{Operation}Query.cs` file and **no** query-object
class — the read lives in the handler. `IReadDbContext` is a two-method port:

```csharp
public interface IReadDbContext
{
    IQueryable<T> Set<T>(bool tracking = false)
        where T : class;

    IQueryable<T> SqlQuery<T>(FormattableString sql);
}
```

`Set<T>()` returns a composable `IQueryable<T>` (no-tracking by default). The handler
chains `.Where()`, `.Include()`, `.OrderBy()/.OrderByDescending()/.ThenBy()`,
`.Skip()/.Take()` and `.Select()`, then awaits an EF async operator
(`.ToListAsync()`, `.FirstOrDefaultAsync()`, `.CountAsync()`, …):

```csharp
public async Task<AdminGetCategoriesResponse> Handle(
    AdminGetCategoriesRequest request,
    CancellationToken cancellationToken)
{
    var categories = await _readDbContext.Set<DomainCategory>()
        .Where(c => !c.IsDeleted)
        .OrderBy(c => c.Name)
        .ToListAsync(cancellationToken);

    var items = categories
        .Select(CategoryResponse.FromDomain)
        .ToList();

    return new AdminGetCategoriesResponse
    {
        Items = items,
        TotalCount = items.Count,
    };
}
```

### Rules

- **EF Core is allowed in the Application layer.** Composition and execution use EF Core
  operators directly (`Include`, `ToListAsync`, `CountAsync`, …) on `Set<T>()`. The
  architecture test (NetArchTest) forbids only a `Notism.Infrastructure` dependency from
  `Notism.Application`; it does **not** forbid `Microsoft.EntityFrameworkCore`.
- **Reads are never shared between handlers.** Each handler composes its own read.
  Recurring predicates (`!IsDeleted`, by-email, `o.UserId == id`) are duplicated inline by
  design — there is **no** shared `Predicates` helper and **no** shared `Queries/` folder.
- **Project or `Include`, as the response needs.** A list/grid read shapes the result with
  `.Select(...)` into a projection type; a read whose response maps off a full entity graph
  declares its navigations with `.Include(...)`.
- **Long or branching reads extract into a local function** inside the same handler file —
  never a shared type. A paged read whose composition is reused for both the count and the
  page does this:

  ```csharp
  IQueryable<DomainOrder> BuildQuery() =>
      _readDbContext.Set<DomainOrder>()
          .Where(filter)
          .OrderByDescending(o => o.CreatedAt)
          .ThenByDescending(o => o.Id)
          .Include("Items.Food.Images")
          .Include(o => o.StatusHistory);

  var totalCount = await BuildQuery().CountAsync(cancellationToken);

  var orders = await BuildQuery()
      .Skip(request.Skip)
      .Take(request.Take)
      .ToListAsync(cancellationToken);
  ```

- **Reporting reads.** Database-native aggregation that LINQ expresses cleanly stays as a
  LINQ aggregate over `Set<T>()` (e.g. a `GroupBy(...).Select(...)` status summary). Where
  the aggregation is SQL-native, use `SqlQuery<T>(FormattableString)` with the SQL inline in
  the handler — interpolated values bind as parameters:

  ```csharp
  FormattableString sql = $"""
      SELECT width_bucket(extract(epoch from "PaidAt"), {epochBoundaries}) - 1 AS "BucketIndex",
             SUM("TotalAmount") AS "Revenue"
      FROM "Orders"
      WHERE "PaymentStatus" = {paidStatus}
        AND "PaidAt" >= {b0}
        AND "PaidAt" < {bn}
      GROUP BY width_bucket(extract(epoch from "PaidAt"), {epochBoundaries})
      """;

  var rows = await _readDbContext.SqlQuery<RevenueBucketRow>(sql).ToListAsync(cancellationToken);
  ```

A brand-new read needs **zero** new repository methods and **zero** new query types — chain
the operators in the handler and go.

## Writes — repositories are the write boundary

`IRepository<T>` is the command/write boundary. A read-modify-write handler loads a
**tracked** aggregate through the read port — `Set<T>(tracking: true)` — mutates it, then
commits via the repository. The same scoped `AppDbContext` backs both the read port and the
repository, so the tracked changes persist:

```csharp
var order = await _readDbContext.Set<DomainOrder>(tracking: true)
        .Where(o => o.Id == request.OrderId && o.UserId == request.UserId)
        .FirstOrDefaultAsync(cancellationToken)
    ?? throw new ResultFailureException(_messages.OrderNotFound);

order.Cancel();
await _orderRepository.SaveChangesAsync();
```

- Tracked write-path loads come from `IReadDbContext.Set<T>(tracking: true)`, not from the
  repository. There is no `GetForUpdateAsync` / `ListForUpdateAsync`, no specification layer,
  and no `FindByExpression` / `FilterByExpression` / `FilterPaged`.
- The repository surface is write-only — see [Repository Pattern](repository-pattern.md).
