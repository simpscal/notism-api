using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;

namespace Notism.Infrastructure.Persistence;

/// <summary>
/// Infrastructure-side implementation of the Application read port. This wires up the entity
/// sets (tracked or no-tracking) and raw SQL; the Application layer composes its queries —
/// including the <c>Include</c> graph and paging — over the queryables returned here and
/// executes them with EF Core async operators directly. A queryable built in tracking mode is
/// tracked by this same context, so a repository <c>SaveChanges</c> on the same scope persists
/// any mutation.
/// </summary>
public partial class AppDbContext : IReadDbContext
{
    IQueryable<T> IReadDbContext.Set<T>(bool tracking)
        => tracking ? Set<T>() : Set<T>().AsNoTracking();

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
        => Database.SqlQuery<T>(sql);
}
