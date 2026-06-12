namespace Notism.Application.Common.Persistence;

public interface IReadDbContext
{
    /// <summary>
    /// Composable queryable over the entity set. No-tracking by default; pass
    /// <paramref name="tracking"/> <c>true</c> for a read-modify-write load.
    /// </summary>
    IQueryable<T> Set<T>(bool tracking = false)
        where T : class;

    IQueryable<T> SqlQuery<T>(FormattableString sql);
}
