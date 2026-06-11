using System.Linq.Expressions;

namespace Notism.Domain.Common.Repositories;

/// <summary>
/// Command/write boundary for an aggregate. Exposes tracked predicate-based loads
/// (for read-modify-write), entity add/remove, and SaveChanges. Read-only and
/// projection queries do NOT live here — they are Application query objects over
/// <c>IReadDbContext</c>.
/// </summary>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// Loads a single TRACKED aggregate matching <paramref name="predicate"/>, with the
    /// navigation graph described by <paramref name="includes"/>. Used by the
    /// read-modify-write handlers to load, mutate, then SaveChanges.
    /// </summary>
    Task<T?> GetForUpdateAsync(Expression<Func<T, bool>> predicate, Action<IncludeBuilder<T>>? includes = null);

    /// <summary>
    /// Loads every TRACKED aggregate matching <paramref name="predicate"/>, with the
    /// navigation graph described by <paramref name="includes"/>. Used by the bulk
    /// read-modify-write handlers.
    /// </summary>
    Task<List<T>> ListForUpdateAsync(Expression<Func<T, bool>> predicate, Action<IncludeBuilder<T>>? includes = null);

    Task<T> AddAsync(T entity);
    T Add(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}
