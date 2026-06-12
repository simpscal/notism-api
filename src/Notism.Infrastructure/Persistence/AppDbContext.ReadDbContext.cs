using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;
using Notism.Domain.Common.Repositories;

namespace Notism.Infrastructure.Persistence;

/// <summary>
/// Infrastructure-side implementation of the Application read port. This is where EF Core's
/// <c>Include</c> graph loading and raw SQL are wired up; the Application layer composes its
/// queries over the queryables returned here and executes them with EF Core async operators
/// directly. A queryable built in tracking mode is tracked by this same context, so a
/// repository <c>SaveChanges</c> on the same scope persists any mutation.
/// </summary>
public partial class AppDbContext : IReadDbContext
{
    IQueryable<T> IReadDbContext.Set<T>(bool tracking)
        => tracking ? Set<T>() : Set<T>().AsNoTracking();

    public IQueryable<T> BuildGraphQuery<T>(
        Expression<Func<T, bool>> predicate,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool tracking = false)
        where T : class
    {
        var query = tracking ? Set<T>() : Set<T>().AsNoTracking();

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

    public IQueryable<T> BuildGraphQuery<T>(
        Expression<Func<T, bool>> predicate,
        int skip,
        int take,
        Action<IncludeBuilder<T>> includes,
        Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
        bool tracking = false)
        where T : class
        => BuildGraphQuery(predicate, includes, orderBy, tracking).Skip(skip).Take(take);

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
        => Database.SqlQuery<T>(sql);
}
