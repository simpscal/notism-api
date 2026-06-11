using Notism.Application.Common.Persistence;
using Notism.Domain.Cart;

namespace Notism.Application.Cart.GetCartItemCount;

/// <summary>
/// Self-contained read: returns the quantities of a user's cart items so the handler can
/// compute the item count and total quantity. Owned by
/// <see cref="GetCartItemCountHandler"/>; the by-user predicate is duplicated inline here
/// rather than shared with any other handler.
/// </summary>
public sealed class GetCartItemCountQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetCartItemCountQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<List<int>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = _readDbContext.Set<CartItem>()
            .Where(c => c.UserId == userId)
            .Select(c => c.Quantity);

        return _readDbContext.ToListAsync(query, cancellationToken);
    }
}
