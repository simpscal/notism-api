using Notism.Application.Common.Persistence;
using Notism.Domain.Cart;

namespace Notism.Application.Cart.GetCartItems;

/// <summary>
/// Self-contained read: loads a user's cart items together with the navigation data the
/// cart response maps over — each item's food (its category, top display image, and
/// customisation groups with their options) and the item's own customisations. The
/// required graph is declared here and applied no-tracking in the Infrastructure read
/// port — the Application layer never calls <c>Include</c>. Owned by
/// <see cref="GetCartItemsHandler"/>; the by-user predicate is duplicated inline here
/// rather than shared with any other handler.
/// </summary>
public sealed class GetCartItemsQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetCartItemsQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<List<CartItem>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _readDbContext.ListWithGraphAsync<CartItem>(
            c => c.UserId == userId,
            includes => includes
                .Include(c => c.Food)
                .Include("Food.Category")
                .Include(c => c.Food.Images.OrderBy(i => i.DisplayOrder).Take(1))
                .Include("Food.CustomisationGroups.Options")
                .Include(c => c.Customisations),
            cancellationToken: cancellationToken);
    }
}
