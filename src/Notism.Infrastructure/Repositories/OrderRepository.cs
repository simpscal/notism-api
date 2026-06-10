using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Infrastructure.Persistence;

namespace Notism.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext appDbContext)
        : base(appDbContext)
    {
    }

    public async Task<OrderStatusBucketCounts> GetDeliveryStatusBucketCountsAsync()
    {
        // Single GROUP BY query: SQL returns at most one row per delivery status.
        var statusCounts = await _dbSet
            .Where(o => o.DeliveryStatus != DeliveryStatus.Cancelled)
            .GroupBy(o => o.DeliveryStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync();

        int CountFor(DeliveryStatus status) =>
            statusCounts.FirstOrDefault(s => s.Status == status)?.Count ?? 0;

        // Fold raw delivery statuses into the dashboard taxonomy. Buckets always
        // present, defaulting to zero when no orders matched that status.
        var newCount = CountFor(DeliveryStatus.OrderPlaced);
        var inProgressCount = CountFor(DeliveryStatus.Preparing) + CountFor(DeliveryStatus.OnTheWay);
        var completedCount = CountFor(DeliveryStatus.Delivered);

        return new OrderStatusBucketCounts(newCount, inProgressCount, completedCount);
    }

    public async Task<OrderWindowAggregate> GetWindowAggregateAsync(DateTime startUtc, DateTime endUtc)
    {
        // Revenue: SUM(TotalAmount) over Paid orders by PaidAt window. Server-side
        // aggregate; SUM of an empty set is coalesced to 0 to keep decimal precision.
        var revenue = await _dbSet
            .Where(o => o.PaymentStatus == PaymentStatus.Paid
                && o.PaidAt != null
                && o.PaidAt >= startUtc
                && o.PaidAt < endUtc)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        // Order count: COUNT over orders by CreatedAt window. Distinct predicate.
        var orderCount = await _dbSet
            .Where(o => o.CreatedAt >= startUtc && o.CreatedAt < endUtc)
            .CountAsync();

        return new OrderWindowAggregate(revenue, orderCount);
    }
}