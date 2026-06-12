using Microsoft.EntityFrameworkCore;

using Notism.Application.Common.Persistence;

namespace Notism.Infrastructure.Persistence;

public partial class AppDbContext : IReadDbContext
{
    IQueryable<T> IReadDbContext.Set<T>(bool tracking)
        => tracking ? Set<T>() : Set<T>().AsNoTracking();

    public IQueryable<T> SqlQuery<T>(FormattableString sql)
        => Database.SqlQuery<T>(sql);
}
