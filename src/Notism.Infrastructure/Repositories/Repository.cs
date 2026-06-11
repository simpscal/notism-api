using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Constants;
using Notism.Domain.Common.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class Repository<T>(AppDbContext appDbContext) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> _dbSet = appDbContext.Set<T>();

    public Task<T?> GetForUpdateAsync(Expression<Func<T, bool>> predicate, Action<IncludeBuilder<T>>? includes = null)
    {
        return ApplyIncludes(includes)
            .Where(predicate)
            .FirstOrDefaultAsync();
    }

    public Task<List<T>> ListForUpdateAsync(Expression<Func<T, bool>> predicate, Action<IncludeBuilder<T>>? includes = null)
    {
        return ApplyIncludes(includes)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public T Add(T entity)
    {
        _dbSet.Add(entity);
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

    private IQueryable<T> ApplyIncludes(Action<IncludeBuilder<T>>? includes)
    {
        var queryable = _dbSet.AsQueryable();

        if (includes is null)
        {
            return queryable;
        }

        var builder = new IncludeBuilder<T>();
        includes(builder);

        foreach (var include in builder.ExpressionIncludes)
        {
            queryable = queryable.Include(include);
        }

        foreach (var stringInclude in builder.StringIncludes)
        {
            queryable = queryable.Include(stringInclude);
        }

        return queryable;
    }
}
