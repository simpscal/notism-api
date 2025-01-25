using Order.Shared.Models;

namespace Order.Domain.Common.Interfaces;

public interface IRepository<T>
{
    Task<T?> FindByExpressionAsync(ISpecification<T> specification);
    Task<IEnumerable<T>> FilterByExpressionAsync(ISpecification<T> specification);
    Task<PagedResult<T>> FilterPagedByExpressionAsync(ISpecification<T> specification, Pagination pagination);
    Task<int> SaveChangesAsync();
}