using Notism.Domain.Common.Repositories;
using Notism.Domain.Order;

namespace Notism.Domain.Order.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Returns order counts folded into the dashboard delivery-status buckets,
    /// computed as a single server-side GROUP BY query. Cancelled orders are
    /// excluded and every bucket is always present (zero when empty).
    /// </summary>
    Task<OrderStatusBucketCounts> GetDeliveryStatusBucketCountsAsync();

    /// <summary>
    /// Returns the SUM of <c>TotalAmount</c> for Paid orders whose <c>PaidAt</c>
    /// falls in <paramref name="startUtc"/>..<paramref name="endUtc"/> (half-open),
    /// and the COUNT of orders whose <c>CreatedAt</c> falls in the same window.
    /// Both are computed as server-side aggregates.
    /// </summary>
    /// <param name="startUtc">Inclusive start of the window (UTC).</param>
    /// <param name="endUtc">Exclusive end of the window (UTC).</param>
    Task<OrderWindowAggregate> GetWindowAggregateAsync(DateTime startUtc, DateTime endUtc);

    /// <summary>
    /// Returns the SUM of <c>TotalAmount</c> of Paid orders bucketed by the
    /// Asia/Ho_Chi_Minh (UTC+7) civil period (year / month / day) of
    /// <c>PaidAt</c>, computed as a single server-side GROUP BY query.
    /// Each row's <see cref="RevenuePeriodTotal.PeriodStartUtc"/> is the inclusive
    /// UTC start of a half-open [periodStartUtc, nextPeriodStartUtc) window.
    /// Only periods containing Paid orders are returned (ordered by period start);
    /// the consuming handler owns zero-filling missing periods.
    /// </summary>
    /// <param name="granularity">The civil-period granularity to bucket by.</param>
    Task<IReadOnlyList<RevenuePeriodTotal>> GetRevenueByPeriodAsync(RevenuePeriodGranularity granularity);
}