using Notism.Application.Common.Persistence;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetOrderById;

/// <summary>
/// Self-contained read: loads a single order by slug — scoped to the requesting user
/// unless the caller is an admin — together with the navigation data the order response
/// maps over (items with their food and food images, and the delivery-status history).
/// The required graph is declared here and applied no-tracking in the Infrastructure read
/// port — the Application layer never calls <c>Include</c>. Owned by
/// <see cref="GetOrderByIdHandler"/>; the predicate and graph are duplicated inline here
/// rather than shared with any other handler.
/// </summary>
public sealed class GetOrderByIdQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetOrderByIdQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<DomainOrder?> ExecuteAsync(
        string slugId,
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        return _readDbContext.FirstWithGraphAsync<DomainOrder>(
            o => o.SlugId == slugId && (o.UserId == userId || isAdmin),
            includes => includes
                .Include("Items.Food.Images")
                .Include(o => o.StatusHistory),
            cancellationToken: cancellationToken);
    }
}
