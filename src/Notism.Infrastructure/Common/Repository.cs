using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Constants;
using Notism.Domain.Common.Interfaces;
using Notism.Shared.Models;

namespace Notism.Infrastructure.Common;

public class Repository<T>(AppDbContext appDbContext) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> _dbSet = appDbContext.Set<T>();

    public Task<T?> FindByExpressionAsync(ISpecification<T> specification)
    {
        var queryable = _dbSet.AsQueryable();

        foreach (var include in specification.Includes)
        {
            queryable = queryable.Include(include);
        }

        queryable = queryable.Where(specification.ToExpression());
        queryable = specification.ApplyOrdering(queryable);

        return queryable.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> FilterByExpressionAsync(ISpecification<T> specification)
    {
        var queryable = _dbSet.AsQueryable();

        foreach (var include in specification.Includes)
        {
            queryable = queryable.Include(include);
        }

        queryable = queryable.Where(specification.ToExpression());

        queryable = specification.ApplyOrdering(queryable);

        return await queryable.ToListAsync();
    }

    public async Task<PagedResult<T>> FilterPagedByExpressionAsync(
        ISpecification<T> specification,
        Pagination pagination)
    {
        var queryable = _dbSet.AsQueryable();

        foreach (var include in specification.Includes)
        {
            queryable = queryable.Include(include);
        }

        queryable = queryable.Where(specification.ToExpression());
        queryable = specification.ApplyOrdering(queryable);
        var totalCount = await queryable.CountAsync();

        queryable = queryable.Skip(pagination.Skip).Take(pagination.Take);
        var items = await queryable.ToListAsync();

        return new PagedResult<T>() { TotalCount = totalCount, Items = items, };
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> SaveChangesAsync()
    {
        var result = await appDbContext.SaveChangesAsync();

        if (result < 1)
        {
            throw new Exception(AppErrorConstants.DumbError);
        }

        return result;
    }
}