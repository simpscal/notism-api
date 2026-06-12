namespace Notism.Application.Common.Persistence;

/// <summary>
/// Persistence port for the Application layer. Every retrieval — read projections and the
/// tracked read-modify-write loads — composes over this port and executes with Entity
/// Framework Core async/LINQ operators directly in the per-handler query logic.
/// <para><see cref="Set{T}"/> returns a composable queryable that is no-tracking by
/// default (tracking on request); the caller chains the <c>Where</c> filter, <c>Include</c>
/// graph, ordering, paging and materialisation it needs in the Application layer. A handler
/// that mutates an aggregate loads it in tracking mode and persists it through the
/// repository's <c>SaveChangesAsync</c> — the read port and the repository share the same
/// context instance, so the tracked entity is saved. Reporting reads whose aggregation is
/// expressed in database-native SQL use <see cref="SqlQuery{T}"/>.</para>
/// </summary>
public interface IReadDbContext
{
    /// <summary>
    /// Returns a composable queryable over the entity set, the composition root for every
    /// read. No-tracking by default; pass <paramref name="tracking"/> <c>true</c> to obtain a
    /// tracked queryable for a read-modify-write load.
    /// </summary>
    IQueryable<T> Set<T>(bool tracking = false)
        where T : class;

    /// <summary>
    /// Returns a queryable over a raw SQL statement, used by the reporting reads whose
    /// aggregation is expressed in database-native SQL.
    /// </summary>
    IQueryable<T> SqlQuery<T>(FormattableString sql);
}
