namespace Notism.Application.Common.Persistence;

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
