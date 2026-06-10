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
    /// Returns the SUM of <c>TotalAmount</c> of Paid orders bucketed into the
    /// half-open UTC ranges defined by <paramref name="boundaries"/>, computed as a
    /// single server-side GROUP BY query (no per-bucket round-trips, no client-side
    /// row materialization). The server performs plain UTC comparison only — it does
    /// NOT derive any time-zone civil period.
    /// <para><paramref name="boundaries"/> is an ascending set of UTC instants
    /// <c>b0 &lt; b1 &lt; ... &lt; bn</c> defining <c>n</c> buckets. Bucket <c>i</c>
    /// covers the half-open range <c>[boundaries[i], boundaries[i+1])</c>: an order
    /// with <c>PaidAt == boundaries[i]</c> falls in bucket <c>i</c>; orders with
    /// <c>PaidAt &lt; boundaries[0]</c> or <c>PaidAt &gt;= boundaries[n]</c> are
    /// excluded. Orders that are not Paid or have a null <c>PaidAt</c> are excluded.</para>
    /// <para>Only buckets that contain Paid orders are returned; the consuming
    /// handler owns zero-filling absent indices into a dense ordered series.</para>
    /// </summary>
    /// <param name="boundaries">Ascending UTC boundary instants (length n+1, n &gt;= 1).</param>
    Task<IReadOnlyList<RevenueBucketTotal>> GetRevenueByBucketsAsync(IReadOnlyList<DateTime> boundaries);
}