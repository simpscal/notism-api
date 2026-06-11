using Notism.Application.Common.Persistence;
using Notism.Domain.Order.Enums;

using DomainOrder = Notism.Domain.Order.Order;

namespace Notism.Application.Order.AdminGetOrderStatusSummary;

/// <summary>
/// Self-contained read for the dashboard delivery-status buckets. Computes the counts
/// as a single server-side GROUP BY over the no-tracking order set, then folds the raw
/// delivery statuses into the dashboard taxonomy. Owned by
/// <see cref="AdminGetOrderStatusSummaryHandler"/> and shared with no other handler.
/// </summary>
public sealed class GetDeliveryStatusBucketCountsQuery
{
    private readonly IReadDbContext _readDbContext;

    public GetDeliveryStatusBucketCountsQuery(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<OrderStatusBucketCounts> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Single GROUP BY query: SQL returns at most one row per delivery status.
        var grouped = _readDbContext.Set<DomainOrder>()
            .Where(o => o.DeliveryStatus != DeliveryStatus.Cancelled)
            .GroupBy(o => o.DeliveryStatus)
            .Select(group => new StatusCount(group.Key, group.Count()));

        var statusCounts = await _readDbContext.ToListAsync(grouped, cancellationToken);

        int CountFor(DeliveryStatus status) =>
            statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;

        // Fold raw delivery statuses into the dashboard taxonomy. Buckets always
        // present, defaulting to zero when no orders matched that status.
        var newCount = CountFor(DeliveryStatus.OrderPlaced);
        var inProgressCount = CountFor(DeliveryStatus.Preparing) + CountFor(DeliveryStatus.OnTheWay);
        var completedCount = CountFor(DeliveryStatus.Delivered);

        return new OrderStatusBucketCounts(newCount, inProgressCount, completedCount);
    }

    private sealed record StatusCount(DeliveryStatus Status, int Count);
}
