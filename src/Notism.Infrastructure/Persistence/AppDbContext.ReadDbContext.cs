using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;

namespace Notism.Infrastructure.Persistence;

/// <summary>
/// Infrastructure-side implementation of the Application read port. A queryable built in
/// tracking mode is tracked by this same context, so a repository <c>SaveChanges</c> on the
/// same scope persists any mutation.
/// </summary>
public partial class AppDbContext : IReadDbContext
{
    IQueryable<T> IReadDbContext.Set<T>(bool tracking)
        => tracking ? Set<T>() : Set<T>().AsNoTracking();

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
        => Database.SqlQuery<T>(sql);
}
