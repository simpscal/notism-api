using Notism.Application.Common.Persistence;
using Notism.Domain.Payment.Enums;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminGetTodaySales;

/// <summary>
/// Self-contained read for the today's-sales window aggregate. Revenue and order count
/// are computed as two distinct server-side aggregates over the no-tracking order set.
/// Owned by <see cref="AdminGetTodaySalesHandler"/> and shared with no other handler.
/// </summary>
public sealed class GetWindowAggregateQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetWindowAggregateQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<OrderWindowAggregate> ExecuteAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        // Revenue: SUM(TotalAmount) over Paid orders by PaidAt window. Server-side
        // aggregate; SUM of an empty set is coalesced to 0 to keep decimal precision.
        var revenueQuery = _readDbContext.Set<DomainOrder>()
            .Where(o => o.PaymentStatus == PaymentStatus.Paid
                && o.PaidAt != null
                && o.PaidAt >= startUtc
                && o.PaidAt < endUtc);

        var revenue = await _readDbContext.SumAsync(revenueQuery, o => (decimal?)o.TotalAmount, cancellationToken);

        var countQuery = _readDbContext.Set<DomainOrder>()
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc);

        var orderCount = await _readDbContext.CountAsync(countQuery, cancellationToken);

        return new OrderWindowAggregate(revenue, orderCount);
    }
}
