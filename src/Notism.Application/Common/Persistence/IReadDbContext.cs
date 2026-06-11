using System.Linq.Expressions;

using Notism.Domain.Common.Repositories;

namespace Notism.Application.Common.Persistence;

/// <summary>
/// Read-only persistence port for the Application layer's query objects.
/// <para>Every <see cref="Set{T}"/> query is no-tracking and is composed with BCL
/// <c>System.Linq</c> operators (<c>Where</c>/<c>OrderBy</c>/<c>Select</c>) inside a
/// per-handler query object, then materialised through the async methods on this
/// port. The Application layer never calls an Entity Framework <c>IQueryable</c>
/// extension directly — query execution lives entirely in the Infrastructure
/// implementation. Reads project straight to their response/projection shape; there
/// is no <c>Include</c> on this port. Command/write loads stay in the repositories.</para>
/// </summary>
public interface IReadDbContext
{
    /// <summary>
    /// Returns a no-tracking queryable over the entity set, the composition root for
    /// every read query object.
    /// </summary>
    IQueryable<T> Set<T>()
        where T : class;

    /// <summary>
    /// Materialises a single no-tracking entity together with the navigation graph the
    /// read maps over. A read whose response builds off the entity's navigations declares
    /// the required graph (typed lambdas and/or string paths) and the Infrastructure
    /// implementation loads it — the Application layer never calls EF's <c>Include</c>.
    /// Use this only for reads that map a full entity graph; projection reads compose a
    /// <c>.Select()</c> over <see cref="Set{T}"/> instead.
    /// </summary>
    Task<T?> FirstWithGraphAsync<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Materialises every matching no-tracking entity together with the declared
    /// navigation graph, in the declared order. Includes are applied in Infrastructure.
    /// </summary>
    Task<List<T>> ListWithGraphAsync<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Materialises a paged no-tracking entity graph: the filter, ordering, total count
    /// and page window all execute server-side; the declared includes are applied in
    /// Infrastructure.
    /// </summary>
    Task<(int TotalCount, List<T> Items)> PagedWithGraphAsync<T>(
        Expression<Func<T, bool>> predicate,
        int skip,
        int take,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Returns a queryable over a raw SQL statement, used by the reporting reads whose
    /// aggregation is expressed in database-native SQL.
    /// </summary>
    IQueryable<T> SqlQuery<T>(FormattableString sql);

    Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default);

    Task<int> SumAsync<T>(IQueryable<T> queryable, Expression<Func<T, int>> selector, CancellationToken cancellationToken = default);

    Task<decimal> SumAsync<T>(IQueryable<T> queryable, Expression<Func<T, decimal?>> selector, CancellationToken cancellationToken = default);
}
