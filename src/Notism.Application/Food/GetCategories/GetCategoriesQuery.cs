using Notism.Application.Common.Persistence;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.GetCategories;

/// <summary>
/// Self-contained read: the non-deleted categories ordered by name, for the public
/// category list. Owned by <see cref="GetCategoriesHandler"/>; the not-deleted
/// predicate is duplicated inline here rather than shared with any other handler.
/// </summary>
public sealed class GetCategoriesQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetCategoriesQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<List<DomainCategory>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainCategory>()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name);

        return _readDbContext.ToListAsync(query, cancellationToken);
    }
}
