using Microsoft.EntityFrameworkCore;

using Notism.Domain.Order;
using Notism.Domain.Order.Enums;
using Notism.Domain.Order.Repositories;
using Notism.Domain.Payment.Enums;
using Notism.Infrastructure.Persistence;
using Notism.Shared.Utilities;

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

    public async Task<IReadOnlyList<RevenuePeriodTotal>> GetRevenueByPeriodAsync(RevenuePeriodGranularity granularity)
    {
        // Bucket Paid revenue by the Asia/Ho_Chi_Minh (UTC+7) civil period of PaidAt.
        // Asia/Ho_Chi_Minh is a fixed UTC+7 offset with no DST (same convention as
        // DayWindow), so shifting PaidAt by +7h yields local civil time without any
        // zone-table dependency. We GROUP BY the integer civil-period components of
        // that shifted timestamp (Year, plus Month/Day per granularity). EF Core
        // translates AddHours + Year/Month/Day to SQL on both Npgsql and SQLite, so
        // the whole aggregate runs as a single server-side GROUP BY query with no
        // client-side row materialization and no N+1.
        var paidOrders = _dbSet
            .Where(o => o.PaymentStatus == PaymentStatus.Paid && o.PaidAt != null)
            .Select(o => new
            {
                Local = o.PaidAt!.Value.AddHours(LocalOffsetHours),
                o.TotalAmount,
            });

        // Group by the civil-period key, SUM(TotalAmount) per group. SUM of decimals
        // is coalesced via the existing (decimal?) ?? 0m pattern; groups are never
        // empty here (GROUP BY only yields populated buckets) but the coalesce keeps
        // decimal precision and guards against a null leak.
        List<RevenuePeriodTotal> totals;
        switch (granularity)
        {
            case RevenuePeriodGranularity.Year:
                totals = (await paidOrders
                        .GroupBy(x => new { x.Local.Year })
                        .Select(g => new
                        {
                            g.Key.Year,
                            Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m,
                        })
                        .ToListAsync())
                    .Select(r => new RevenuePeriodTotal(LocalPeriodStartUtc(r.Year, 1, 1), r.Revenue))
                    .ToList();
                break;

            case RevenuePeriodGranularity.Month:
                totals = (await paidOrders
                        .GroupBy(x => new { x.Local.Year, x.Local.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m,
                        })
                        .ToListAsync())
                    .Select(r => new RevenuePeriodTotal(LocalPeriodStartUtc(r.Year, r.Month, 1), r.Revenue))
                    .ToList();
                break;

            case RevenuePeriodGranularity.Day:
            default:
                totals = (await paidOrders
                        .GroupBy(x => new { x.Local.Year, x.Local.Month, x.Local.Day })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            g.Key.Day,
                            Revenue = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m,
                        })
                        .ToListAsync())
                    .Select(r => new RevenuePeriodTotal(LocalPeriodStartUtc(r.Year, r.Month, r.Day), r.Revenue))
                    .ToList();
                break;
        }

        return totals
            .OrderBy(t => t.PeriodStartUtc)
            .ToList();
    }

    private static readonly int LocalOffsetHours = (int)DayWindow.HoChiMinhOffset.TotalHours;

    // Reconstruct the inclusive UTC start of a UTC+7 civil period from its local
    // civil-date components: build local midnight, then shift back by the offset.
    private static DateTime LocalPeriodStartUtc(int year, int month, int day)
    {
        var localMidnight = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
        return DateTime.SpecifyKind(localMidnight - DayWindow.HoChiMinhOffset, DateTimeKind.Utc);
    }
}