using System.Linq.Expressions;

using Notism.Application.Common.Persistence;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.GetOrders;

/// <summary>
/// Self-contained paged read for a user's orders. Loads each order together with the
/// navigation data the order response maps over — its items (each item's food and food
/// images) and its delivery-status history. The required graph is declared here and
/// applied no-tracking in the Infrastructure read port — the Application layer never calls
/// <c>Include</c>.
/// <para>The ordering is deterministic: most recent first by <c>CreatedAt</c> with the
/// primary key as a tie-breaker, so no row is duplicated or skipped across page
/// boundaries; the filter, ordering, count and page window all execute server-side so
/// paged counts stay correct. Owned by <see cref="GetOrdersHandler"/>; the predicate is
/// composed by the handler and the graph/ordering are duplicated inline here rather than
/// shared with any other handler.</para>
/// </summary>
public sealed class GetOrdersQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetOrdersQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<(int TotalCount, List<DomainOrder> Items)> ExecuteAsync(
        Expression<Func<DomainOrder, bool>> filter,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return _readDbContext.PagedWithGraphAsync<DomainOrder>(
            filter,
            skip,
            take,
            includes => includes
                .Include("Items.Food.Images")
                .Include(o => o.StatusHistory),
            query => query
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id),
            cancellationToken);
    }
}
