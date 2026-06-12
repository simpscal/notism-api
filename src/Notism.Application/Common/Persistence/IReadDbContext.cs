using System.Linq.Expressions;

using Notism.Domain.Common.Repositories;

namespace Notism.Application.Common.Persistence;

/// <summary>
/// Persistence port for the Application layer. Every retrieval — read projections and the
/// tracked read-modify-write loads — composes over this port and executes with Entity
/// Framework Core async/LINQ operators directly in the per-handler query logic.
/// <para><see cref="Set{T}"/> returns a composable queryable that is no-tracking by
/// default (tracking on request); <see cref="BuildGraphQuery{T}(Expression{Func{T, bool}}, Action{IncludeBuilder{T}}, Func{IQueryable{T}, IQueryable{T}}?, bool)"/>
/// applies the declared navigation graph (the <c>Include</c> calls live in the
/// Infrastructure implementation). A handler that mutates an aggregate loads it in tracking
/// mode and persists it through the repository's <c>SaveChangesAsync</c> — the read port and
/// the repository share the same context instance, so the tracked entity is saved. Reporting
/// reads whose aggregation is expressed in database-native SQL use <see cref="SqlQuery{T}"/>.</para>
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
    /// Builds a composable queryable with the declared navigation graph applied (typed
    /// lambdas and/or string paths; the <c>Include</c> calls are applied in Infrastructure),
    /// the predicate filtered and optional ordering applied. No-tracking by default; pass
    /// <paramref name="tracking"/> <c>true</c> for a read-modify-write load.
    /// </summary>
    IQueryable<T> BuildGraphQuery<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool tracking = false)
        where T : class;

    /// <summary>
    /// Builds a paged composable queryable: the declared navigation graph, predicate and
    /// ordering as in the non-paged overload, plus the <paramref name="skip"/>/<paramref name="take"/>
    /// page window. No-tracking by default.
    /// </summary>
    IQueryable<T> BuildGraphQuery<T>(
        Expression<Func<T, bool>> predicate,
        int skip,
        int take,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool tracking = false)
        where T : class;

    /// <summary>
    /// Returns a queryable over a raw SQL statement, used by the reporting reads whose
    /// aggregation is expressed in database-native SQL.
    /// </summary>
    IQueryable<T> SqlQuery<T>(FormattableString sql);
}
