namespace Notism.Domain.Common.Repositories;

public interface IRepository<T>
    where T : class
{
    Task<T> AddAsync(T entity);
    T Add(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}
