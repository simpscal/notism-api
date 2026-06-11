using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Domain.Common.Repositories;

namespace Notism.Infrastructure.Persistence;

/// <summary>
/// Infrastructure-side implementation of the Application read port. This is the ONLY
/// place EF Core's async <c>IQueryable</c> operators are invoked on behalf of the
/// Application query objects; the Application layer composes queries with BCL LINQ and
/// materialises them exclusively through these methods.
/// </summary>
public partial class AppDbContext : IReadDbContext
{
    IQueryable<T> IReadDbContext.Set<T>()
        => Set<T>().AsNoTracking();

    public Task<T?> FirstWithGraphAsync<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
        where T : class
        => BuildGraphQuery(predicate, includes, orderBy).FirstOrDefaultAsync(cancellationToken);

    public Task<List<T>> ListWithGraphAsync<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
        where T : class
        => BuildGraphQuery(predicate, includes, orderBy).ToListAsync(cancellationToken);

    public async Task<(int TotalCount, List<T> Items)> PagedWithGraphAsync<T>(
        Expression<Func<T, bool>> predicate,
        int skip,
        int take,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var query = BuildGraphQuery(predicate, includes, orderBy);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);

        return (totalCount, items);
    }

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
        => Database.SqlQuery<T>(sql);

    public Task<List<T>> ToListAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
        => queryable.ToListAsync(cancellationToken);

    public Task<T?> FirstOrDefaultAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
        => queryable.FirstOrDefaultAsync(cancellationToken);

    public Task<int> CountAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
        => queryable.CountAsync(cancellationToken);

    public Task<bool> AnyAsync<T>(IQueryable<T> queryable, CancellationToken cancellationToken = default)
        => queryable.AnyAsync(cancellationToken);

    public Task<int> SumAsync<T>(IQueryable<T> queryable, Expression<Func<T, int>> selector, CancellationToken cancellationToken = default)
        => queryable.SumAsync(selector, cancellationToken);

    public async Task<decimal> SumAsync<T>(IQueryable<T> queryable, Expression<Func<T, decimal?>> selector, CancellationToken cancellationToken = default)
        => await queryable.SumAsync(selector, cancellationToken) ?? 0m;

    private IQueryable<T> BuildGraphQuery<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy)
        where T : class
    {
        var query = Set<T>().AsNoTracking();

        var builder = new IncludeBuilder<T>();
        includes(builder);

        foreach (var include in builder.ExpressionIncludes)
        {
            query = query.Include(include);
        }

        foreach (var stringInclude in builder.StringIncludes)
        {
            query = query.Include(stringInclude);
        }

        query = query.Where(predicate);

        if (orderBy is not null)
        {
            query = orderBy(query);
        }

        return query;
    }
}
