using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Constants;
using Notism.Domain.Common.Repositories;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class Repository<T>(AppDbContext appDbContext) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> _dbSet = appDbContext.Set<T>();

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
}
