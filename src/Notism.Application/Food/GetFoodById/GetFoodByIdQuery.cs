using Notism.Application.Common.Persistence;

using DomainFood = Notism.Domain.Food.Food;

namespace Notism.Application.Food.GetFoodById;

/// <summary>
/// Self-contained read: loads a single non-deleted food together with the navigation data
/// the detail response maps over (images, category, customisation groups and their
/// options). The required graph is declared here and applied no-tracking in the
/// Infrastructure read port — the Application layer never calls <c>Include</c>. Owned by
/// <see cref="GetFoodByIdHandler"/>; the by-id/not-deleted predicate is duplicated inline
/// here rather than shared with any other handler.
/// </summary>
public sealed class GetFoodByIdQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetFoodByIdQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<DomainFood?> ExecuteAsync(Guid foodId, CancellationToken cancellationToken = default)
    {
        return _readDbContext.FirstWithGraphAsync<DomainFood>(
            f => f.Id == foodId && !f.IsDeleted,
            includes => includes
                .Include(f => f.Images)
                .Include(f => f.Category!)
                .Include("CustomisationGroups.Options"),
            cancellationToken: cancellationToken);
    }
}
