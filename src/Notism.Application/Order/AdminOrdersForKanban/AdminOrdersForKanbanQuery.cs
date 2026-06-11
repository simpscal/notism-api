using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;
using Notism.Domain.Payment.Enums;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminOrdersForKanban;

/// <summary>
/// Self-contained paged read for the admin kanban board: filters by delivery status and
/// optional payment status, ordered most-recent first. Loads each order with the user and
/// items the kanban card maps over; the required graph is declared here and applied
/// no-tracking in the Infrastructure read port — the Application layer never calls
/// <c>Include</c>. Owned by <see cref="AdminOrdersForKanbanHandler"/>; every predicate,
/// ordering and graph is duplicated inline here rather than shared with any other handler.
/// </summary>
public sealed class AdminOrdersForKanbanQuery
{
    private readonly IReadDbContext _readDbContext;

    public AdminOrdersForKanbanQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public Task<(int TotalCount, List<DomainOrder> Items)> ExecuteAsync(
        DeliveryStatus deliveryStatus,
        PaymentStatus? paymentStatus,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        return _readDbContext.PagedWithGraphAsync<DomainOrder>(
            order => order.DeliveryStatus == deliveryStatus
                && (paymentStatus == null || order.PaymentStatus == paymentStatus),
            skip,
            take,
            includes => includes
                .Include(o => o.User!)
                .Include(o => o.Items),
            query => query.OrderByDescending(o => o.CreatedAt),
            cancellationToken);
    }
}
