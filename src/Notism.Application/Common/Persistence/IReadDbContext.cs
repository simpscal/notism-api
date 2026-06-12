namespace Notism.Application.Common.Persistence;

public interface IReadDbContext
{
    IQueryable<T> Set<T>(bool tracking = false)
        where T : class;

    IQueryable<T> SqlQuery<T>(FormattableString sql);
}