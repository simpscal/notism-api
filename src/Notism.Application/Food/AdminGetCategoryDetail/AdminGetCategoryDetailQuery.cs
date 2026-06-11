using Notism.Application.Common.Persistence;

using DomainCategory = Notism.Domain.Food.Category;

namespace Notism.Application.Food.AdminGetCategoryDetail;

/// <summary>
/// Self-contained read: a single non-deleted category by id, for the admin detail view.
/// Owned by <see cref="AdminGetCategoryDetailHandler"/>; the by-id/not-deleted predicate
/// is duplicated inline here rather than shared with any other handler.
/// </summary>
public sealed class AdminGetCategoryDetailQuery
{
    private readonly IReadDbContext _readDbContext;

    public AdminGetCategoryDetailQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<DomainCategory?> ExecuteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<DomainCategory>()
            .Where(c => c.Id == categoryId && !c.IsDeleted);

        return _readDbContext.FirstOrDefaultAsync(query, cancellationToken);
    }
}
