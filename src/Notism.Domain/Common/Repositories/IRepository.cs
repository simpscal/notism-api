namespace Notism.Domain.Common.Repositories;

/// <summary>
/// Write boundary for an aggregate: entity add/remove and SaveChanges, plus any sanctioned
/// bulk mutations declared on a derived repository. Retrieval — read projections and the
/// tracked read-modify-write loads — does NOT live here; it is composed over
/// <c>IReadDbContext</c> in the Application handlers.
/// </summary>
public interface IRepository<T>
    where T : class
{
    Task<T> AddAsync(T entity);
    T Add(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}
