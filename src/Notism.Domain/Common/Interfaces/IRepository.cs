using System.Linq.Expressions;

using Notism.Shared.Models;

namespace Notism.Domain.Common.Interfaces;

public interface IRepository<T>
{
    Task<T?> FindByExpressionAsync(ISpecification<T> specification);
    Task<TProjection?> FindByExpressionAsync<TProjection>(ISpecification<T> specification, Expression<Func<T, TProjection>> select);

    Task<IEnumerable<T>> FilterByExpressionAsync(ISpecification<T> specification);
    Task<IEnumerable<TProjection>> FilterByExpressionAsync<TProjection>(ISpecification<T> specification, Expression<Func<T, TProjection>> select);

    Task<PagedResult<T>> FilterPagedByExpressionAsync(ISpecification<T> specification, Pagination pagination);
    Task<PagedResult<TProjection>> FilterPagedByExpressionAsync<TProjection>(ISpecification<T> specification, Pagination pagination, Expression<Func<T, TProjection>> select);
    Task<T> AddAsync(T entity);
    T Add(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();
}