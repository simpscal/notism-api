# Read Queries

Reads and writes use two different persistence paths.

## Reads — per-handler query objects over `IReadDbContext`

Every read handler owns its **own** query object, co-located with the handler
(`{Feature}/{Operation}/{Operation}Query.cs`). The query object composes its LINQ over
the Application read port `IReadDbContext` and materialises through that port. There is
no shared query layer.

```csharp
public sealed class AdminGetCategoriesQuery
{
    private readonly IReadDbContext _readDbContext;

    public AdminGetCategoriesQuery(IReadDbContext readDbContext) => _readDbContext = readDbContext;

    public Task<List<Category>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<Category>()
            .Where(c => !c.IsDeleted)          // predicate duplicated inline — never shared
            .OrderBy(c => c.Name);

        return _readDbContext.ToListAsync(query, cancellationToken);
    }
}
```

### Rules

- **Query objects are never shared between handlers.** Two handlers that need the same
  read each get their own query object. Trivial predicate duplication (`!IsDeleted`,
  by-email, `o.UserId == id`) is duplicated inline by design — there is **no** shared
  `Predicates` helper and **no** shared `{Feature}/Queries/` folder of reusable queries.
- **Application never calls an EF `IQueryable` extension.** Composition uses BCL
  `System.Linq` (`Where`/`OrderBy`/`Select`); execution (`ToListAsync`, `CountAsync`,
  `SumAsync`, …) goes through `IReadDbContext`. EF lives only in the Infrastructure
  implementation. An architecture test enforces that `Notism.Application` references
  neither `Notism.Infrastructure` nor `Microsoft.EntityFrameworkCore`.
- **Project, don't `Include`.** Pure-read/projection queries shape the result with
  `.Select(...)` into the response/projection type. The Application layer never calls
  `Include`.
- **Graph reads declare their navigations.** A read whose response maps off a full entity
  graph (e.g. an order with its items, food images and status history) uses
  `IReadDbContext.FirstWithGraphAsync` / `ListWithGraphAsync` / `PagedWithGraphAsync`,
  declaring the required navigations via the include builder. The includes are applied
  **no-tracking in Infrastructure** — still no `Include` in Application.
- **Reporting reads** whose aggregation is database-native SQL use
  `IReadDbContext.SqlQuery<T>(FormattableString)`, with the SQL preserved verbatim in the
  query object.

A brand-new read needs **zero** new repository methods — add a query object and go.

## Writes — repositories are the write boundary

`IRepository<T>` is the command/write boundary. Read-modify-write handlers load a
**tracked** aggregate by a richer-than-PK predicate, mutate it, and `SaveChanges`:

```csharp
var food = await _foodRepository.GetForUpdateAsync(
    f => f.Id == request.FoodId && !f.IsDeleted,
    includes => includes.Include("CustomisationGroups.Options"));

food.AddCustomisationGroup(group);
await _foodRepository.SaveChangesAsync();
```

- `GetForUpdateAsync` / `ListForUpdateAsync` return tracked, optionally graph-included
  aggregates for mutation. There is no specification layer and no
  `FindByExpression` / `FilterByExpression` / `FilterPaged` on the repository.
- Read-only projection queries do **not** live on the repository — they are Application
  query objects over `IReadDbContext`.
